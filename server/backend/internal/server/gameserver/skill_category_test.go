package gameserver

import (
	"math/rand"
	"testing"
)

func TestSkillCategory56MapsToPhysSpec(t *testing.T) {
	if !skillUsesPhysical(1) || !skillUsesPhysical(5) || skillUsesPhysical(2) || skillUsesPhysical(6) {
		t.Fatal("physical map")
	}
	if !skillUsesSpecial(2) || !skillUsesSpecial(6) || skillUsesSpecial(1) || skillUsesSpecial(5) {
		t.Fatal("special map")
	}
}

func TestHolySpiritFlashUsesSpecialNotPhysical(t *testing.T) {
	// 特攻型：Atk=50、SpAtk=200。本表圣灵魔闪光 Category=6，应对齐特攻。
	rand.Seed(1)
	holy := calcSkillDamage(70, 160, 6, 50, 100, 200, 100)
	rand.Seed(1)
	qianlie := calcSkillDamage(70, 140, 2, 50, 100, 200, 100)
	if holy <= qianlie {
		t.Fatalf("圣灵魔闪光(160特) should > 千烈虚光闪(140特): holy=%d qian=%d", holy, qianlie)
	}
	rand.Seed(1)
	holyAsPhys := calcSkillDamage(70, 160, 1, 50, 100, 200, 100)
	if holy <= holyAsPhys {
		t.Fatalf("Category6 must use SpAtk: special=%d physPath=%d", holy, holyAsPhys)
	}
}
