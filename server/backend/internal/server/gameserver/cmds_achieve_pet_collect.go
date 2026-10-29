package gameserver

import (
	"log"

	"niaohao/server/internal/cmdname"
	"niaohao/server/internal/store"
	"niaohao/server/internal/tableloader"
)

const petCollectAchieveBranch = 1 // AchieveXML「精灵收集 / 收集数量」

// syncPetCollectAchieve 按背包+仓库不重复种族数点亮 Branch1 收集成就。
func (s *Server) syncPetCollectAchieve(uid int64) {
	if s == nil || s.cfg.Store == nil || s.cfg.Catalog == nil || uid <= 0 {
		return
	}
	species := s.countOwnedPetSpecies(uid)
	if species <= 0 {
		return
	}
	rules := s.cfg.Catalog.AchieveRulesOfBranch(petCollectAchieveBranch)
	if len(rules) == 0 {
		return
	}
	already, _ := s.cfg.Store.ListAchieveRulesOfBranch(uid, petCollectAchieveBranch)
	done := make(map[int]bool, len(already))
	oldMask := 0
	for _, row := range already {
		if row.Completed {
			done[row.RuleID] = true
			if bit := tableloader.AchieveCompleteBit(petCollectAchieveBranch, row.RuleID); bit >= 0 && bit < 32 {
				oldMask |= 1 << bit
			}
		}
	}
	newly := false
	newMask := oldMask
	pointSum := 0
	for i := range rules {
		r := &rules[i]
		if r.Threshold <= 0 || species < r.Threshold {
			continue
		}
		if done[r.RuleID] {
			continue
		}
		_ = s.cfg.Store.UpsertAchieveRule(uid, store.AchieveRuleRow{
			BranchID:  petCollectAchieveBranch,
			RuleID:    r.RuleID,
			Progress:  species,
			Completed: true,
			Claimed:   r.SpeNameBonus == 0,
		})
		if r.SpeNameBonus > 0 {
			_ = s.cfg.Store.GrantTitle(uid, r.SpeNameBonus)
			s.pushAchieveTitle(uint32(uid), uint32(r.SpeNameBonus))
		}
		if bit := tableloader.AchieveCompleteBit(petCollectAchieveBranch, r.RuleID); bit >= 0 && bit < 32 {
			newMask |= 1 << bit
		}
		pointSum += r.AchievementPoint
		newly = true
	}
	if !newly {
		// 未新完成：写入当前进度，供面板「a / b」
		for i := range rules {
			r := &rules[i]
			if done[r.RuleID] {
				continue
			}
			_ = s.cfg.Store.UpsertAchieveRule(uid, store.AchieveRuleRow{
				BranchID: petCollectAchieveBranch,
				RuleID:   r.RuleID,
				Progress: species,
			})
			break
		}
		return
	}
	s.pushAchieveInform(uint32(uid), petCollectAchieveBranch, pointSum, species, oldMask, newMask)
	log.Printf("[achieve] UID=%d petCollect species=%d %s", uid, species, cmdname.Format(3451))
}

func (s *Server) countOwnedPetSpecies(uid int64) int {
	seen := make(map[int]bool)
	add := func(pets []store.Pet) {
		for _, p := range pets {
			pid := p.PetID
			if pid <= 0 {
				continue
			}
			if s.cfg.Catalog != nil {
				pid = s.cfg.Catalog.EvolutionBaseForm(pid)
			}
			if pid > 0 {
				seen[pid] = true
			}
		}
	}
	if bag, err := s.cfg.Store.ListBagPets(uid); err == nil {
		add(bag)
	}
	if stor, err := s.cfg.Store.ListStoragePets(uid); err == nil {
		add(stor)
	}
	return len(seen)
}
