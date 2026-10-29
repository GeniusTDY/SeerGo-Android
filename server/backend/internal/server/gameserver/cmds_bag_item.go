package gameserver

import (
	"encoding/binary"
	"log"
	"math/rand"
	"strconv"

	"niaohao/server/internal/cmdname"
	"niaohao/server/internal/store"
)

// 背包直接使用（BagPanel COMMON_USE_ITEMS / USE_BAG_ITEM 80009）
const (
	bagItemRandomPet = 403025 // 神秘精灵胶囊：背包满 6 只后可开，随机发 1 只基础精灵
	bagItemGoldLucky = 403026 // 金豆福袋/盲盒：随机 8–30 金豆
)

var bagItemGoldLuckyRange = [2]int{8, 30}

// 随机精灵池（本前端常见低阶精灵，避免超限 ID）
var bagItemRandomPetPool = []int{
	1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34, 38, 41, 44, 47, 50, 53, 56, 59,
	62, 65, 68, 71, 74, 77, 80, 83, 86, 89, 92, 95, 98, 101, 104, 107, 110,
}

// handleUseBagItem CMD 80009：请求 itemID(4)；成功空 ACK，并推 8004/1106。
func (s *Server) handleUseBagItem(c *Client, uid uint32, body []byte) {
	itemID := uint32(0)
	if len(body) >= 4 {
		itemID = binary.BigEndian.Uint32(body[0:4])
	}
	ack := func() { s.send(c, 80009, uid, 0, nil) }
	if s.cfg.Store == nil || itemID == 0 {
		ack()
		return
	}
	switch itemID {
	case bagItemGoldLucky:
		s.useBagGoldLucky(c, uid, ack)
	case bagItemRandomPet:
		s.useBagRandomPet(c, uid, ack)
	default:
		ack()
		log.Printf("[CMD] OK     %s UID=%d item=%d (no-op)", cmdname.Format(80009), uid, itemID)
	}
}

func (s *Server) useBagGoldLucky(c *Client, uid uint32, ack func()) {
	if err := s.cfg.Store.ConsumeItem(int64(uid), bagItemGoldLucky, 1); err != nil {
		ack()
		s.sendAlert(int64(uid), "没有可用的金豆福袋")
		log.Printf("[CMD] OK     %s UID=%d gold-bag no stock", cmdname.Format(80009), uid)
		return
	}
	lo, hi := bagItemGoldLuckyRange[0], bagItemGoldLuckyRange[1]
	if hi < lo {
		lo, hi = hi, lo
	}
	n := lo + rand.Intn(hi-lo+1)
	if err := s.cfg.Store.AddGold(int64(uid), n); err != nil {
		_ = s.cfg.Store.AddItem(int64(uid), bagItemGoldLucky, 1)
		ack()
		log.Printf("[CMD] WARN  %s UID=%d AddGold: %v", cmdname.Format(80009), uid, err)
		return
	}
	ack()
	s.send(c, 8004, uid, 0, buildBossMonster8004Body(0, 0, 0, mailRewardItemGold, uint32(n)))
	s.pushGoldBalance1106(c, uid)
	s.sendAlert(int64(uid), "获得金豆×"+strconv.Itoa(n))
	log.Printf("[CMD] OK     %s UID=%d gold-bag +%d", cmdname.Format(80009), uid, n)
}

func (s *Server) useBagRandomPet(c *Client, uid uint32, ack func()) {
	nBag, _ := s.cfg.Store.CountBagPets(int64(uid))
	if nBag < store.MaxBagPets {
		ack()
		s.sendAlert(int64(uid), "请先让精灵背包凑满6只后再使用")
		log.Printf("[CMD] OK     %s UID=%d random-pet need bag=%d", cmdname.Format(80009), uid, nBag)
		return
	}
	if err := s.cfg.Store.ConsumeItem(int64(uid), bagItemRandomPet, 1); err != nil {
		ack()
		s.sendAlert(int64(uid), "没有可用的神秘精灵胶囊")
		return
	}
	petID := bagItemRandomPetPool[rand.Intn(len(bagItemRandomPetPool))]
	if s.cfg.Catalog != nil {
		for i := 0; i < 8; i++ {
			cand := bagItemRandomPetPool[rand.Intn(len(bagItemRandomPetPool))]
			if s.cfg.Catalog.PetBase(cand) != nil {
				petID = cand
				break
			}
		}
	}
	catch, err := s.grantNewPet(int64(uid), petID, 1)
	if err != nil {
		_ = s.cfg.Store.AddItem(int64(uid), bagItemRandomPet, 1)
		ack()
		log.Printf("[CMD] WARN  %s UID=%d grant: %v", cmdname.Format(80009), uid, err)
		return
	}
	ack()
	s.pushBossMonster8004(c, uid, uint32(petID), uint32(catch))
	log.Printf("[CMD] OK     %s UID=%d random-pet id=%d catch=%d", cmdname.Format(80009), uid, petID, catch)
}
