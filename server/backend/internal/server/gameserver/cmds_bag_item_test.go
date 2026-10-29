package gameserver

import (
	"testing"

	"niaohao/server/internal/store"
	"niaohao/server/internal/tableloader"
)

func TestApplyPetItemMetaMonAttrReset(t *testing.T) {
	p := &store.Pet{PetID: 1, Level: 40, Exp: 123, DV: 10, Nature: 3, EV: [6]int{10, 20, 30, 40, 50, 60}}
	meta := tableloader.ItemMeta{MonAttrReset: true}
	if !applyPetItemMeta(p, meta) {
		t.Fatal("expected change")
	}
	if p.Level != 1 || p.Exp != 0 {
		t.Fatalf("level/exp = %d/%d", p.Level, p.Exp)
	}
	if p.EV != [6]int{} {
		t.Fatalf("ev=%v", p.EV)
	}
	if p.DV < 0 || p.DV > 31 {
		t.Fatalf("dv=%d", p.DV)
	}
	if p.Nature < 0 || p.Nature > 24 {
		t.Fatalf("nature=%d", p.Nature)
	}
}

func TestBagItemGoldLuckyRange(t *testing.T) {
	lo, hi := bagItemGoldLuckyRange[0], bagItemGoldLuckyRange[1]
	if lo != 8 || hi != 30 {
		t.Fatalf("range=%d-%d want 8-30", lo, hi)
	}
	if bagItemGoldLucky != 403026 || bagItemRandomPet != 403025 {
		t.Fatalf("ids %d %d", bagItemGoldLucky, bagItemRandomPet)
	}
}
