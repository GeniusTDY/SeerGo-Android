package gameserver

import (
	"testing"

	"niaohao/server/internal/store"
	"niaohao/server/internal/tableloader"
)

func TestChallengeBossCatchableTriggerPets(t *testing.T) {
	s := &Server{}
	for _, id := range []int{74, 463, 474, 153, 59, 414, 178} {
		if !s.challengeBossCatchable(id) {
			t.Fatalf("pet %d should be catchable", id)
		}
	}
	for _, id := range []int{47, 34, 42, 69, 88, 113, 70, 501, 502, 187} {
		if s.challengeBossCatchable(id) {
			t.Fatalf("SPT pet %d should NOT be catchable", id)
		}
	}
	if s.challengeBossCatchable(petIDPuni) {
		t.Fatal("puni should not be catchable")
	}
}

func TestGalaxyPortalTaskMapping(t *testing.T) {
	if galaxyPortalTaskIDForBoss(47) != 301 {
		t.Fatal("蘑菇怪 → 301")
	}
	if galaxyPortalTaskIDForBoss(113) != 308 {
		t.Fatal("雷纳多 → 308")
	}
	if galaxyPortalTaskIDForBoss(264) != 312 {
		t.Fatal("奈尼芬多 → 312")
	}
	if galaxyPortalTaskIDForBoss(421) != 316 {
		t.Fatal("厄尔塞拉 → 316")
	}
	if galaxyPortalTaskIDForBoss(74) != 0 {
		t.Fatal("果冻鸭无传送阵任务")
	}
}

func TestApplyPetItemMetaMaxHPUp(t *testing.T) {
	p := &store.Pet{PetID: 1, Level: 10, DV: 15}
	meta := tableloader.ItemMeta{MaxHPUp: 5}
	if !applyPetItemMeta(p, meta) {
		t.Fatal("expected change")
	}
	if p.Bonus[0] != 5 {
		t.Fatalf("bonus=%d", p.Bonus[0])
	}
	p.Bonus[0] = 18
	if !applyPetItemMeta(p, meta) {
		t.Fatal("expected partial add")
	}
	if p.Bonus[0] != petMaxHPBonusCap {
		t.Fatalf("cap bonus=%d", p.Bonus[0])
	}
	if applyPetItemMeta(p, meta) {
		t.Fatal("at cap should not change")
	}
}

func TestResolveBossLugeluoMap411(t *testing.T) {
	pid, lv, name := resolveChallengeBoss(411, 0)
	if pid != 463 || lv != 17 || name != "鲁格洛" {
		t.Fatalf("got pet=%d lv=%d name=%s", pid, lv, name)
	}
	pid, _, name = resolveChallengeBoss(411, 1)
	if pid != 475 || name != "该隐" {
		t.Fatalf("param2=1 want 该隐 got pet=%d name=%s", pid, name)
	}
	// 未知 param 回落 0 号位，不再变成塔奇拉顿
	pid, _, name = resolveChallengeBoss(411, 99)
	if pid != 463 || name != "鲁格洛" {
		t.Fatalf("miss param want 鲁格洛 got pet=%d name=%s", pid, name)
	}
	pid, _, name = resolveChallengeBoss(56, 3)
	if pid != 432 || name != "杰西卡" {
		t.Fatalf("map56 p3 want 杰西卡 got pet=%d name=%s", pid, name)
	}
	pid, _, name = resolveChallengeBoss(101, 0)
	if pid != 398 || name != "杰拉德" {
		t.Fatalf("map101 want 杰拉德 got pet=%d name=%s", pid, name)
	}
}
