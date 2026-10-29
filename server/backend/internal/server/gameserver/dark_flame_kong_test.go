package gameserver

import "testing"

func TestBoundGodPetDefaultOff(t *testing.T) {
	if isBoundGodPet(2637) {
		t.Fatal("default build must not enable bound god without boundgod package link")
	}
	st := &BattleState{PlayerPetID: 2637}
	if playerHasInfinitePP(st) || forcePlayerAlwaysFirst(st) || enemyOffenseNullified(st) {
		t.Fatal("default rules off")
	}
	var dmg uint32 = 50
	crit := true
	applyBoundGodPetIncomingGuard(2637, &dmg, &crit)
	if dmg != 50 || !crit {
		t.Fatalf("guard no-op want dmg=50, got %d crit=%v", dmg, crit)
	}
}
