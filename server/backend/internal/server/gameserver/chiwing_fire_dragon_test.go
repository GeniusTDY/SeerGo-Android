package gameserver

import (
	"testing"

	"niaohao/server/internal/tableloader"
)

func TestChiWingMapBossResolve(t *testing.T) {
	pid, lv, name := resolveChallengeBoss(16, 1)
	if pid != petIDChiWingFireDragon || lv != 100 || name != "炽翼火龙" {
		t.Fatalf("got pet=%d lv=%d name=%q", pid, lv, name)
	}
	// 上古炎兽仍在 param0
	pid0, _, _ := resolveChallengeBoss(16, 0)
	if pid0 != 393 {
		t.Fatalf("map16 p0=%d want 393", pid0)
	}
}

func TestChiWingBossOpenRules(t *testing.T) {
	st := &BattleState{
		EnemyID: 1238, EnemyLevel: 100,
		EnemyHP: 1, EnemyMaxHP: 1,
		EnemyAtk: 10, EnemySpAtk: 10,
		MapID: 16, BossRegion: 1,
	}
	applyBossOpenBattleRules(st)
	if st.EnemyMaxHP != 10000 || st.EnemyHP != 10000 {
		t.Fatalf("hp=%d/%d want 10000", st.EnemyHP, st.EnemyMaxHP)
	}
	if st.EnemyAtk != 500 || st.EnemySpAtk != 500 {
		t.Fatalf("atk=%d spa=%d want 500", st.EnemyAtk, st.EnemySpAtk)
	}
}

func TestChiWingBossBurn(t *testing.T) {
	st := &BattleState{EnemyID: 1238, PlayerPetID: 1}
	applyChiWingBossBurn(st, true)
	if st.PlayerStatus.Burn == 0 {
		t.Fatal("expected burn")
	}
	st2 := &BattleState{EnemyID: 1238, PlayerPetID: 1238}
	applyChiWingBossBurn(st2, true)
	if st2.PlayerStatus.Burn > 0 {
		t.Fatal("reward pet should be status immune")
	}
}

func TestChiWingRewardPetImmunities(t *testing.T) {
	if !isStatusImmunePet(1238) || !isStatDropImmunePet(1238) {
		t.Fatal("status/stat immune")
	}
	for _, sid := range []uint32{10036, 10210, 20215, 20143} {
		if !petImmuneToSkill(1238, sid) {
			t.Fatalf("skill %d should be immune", sid)
		}
	}
	if petImmuneToSkill(1, 10036) {
		t.Fatal("normal pet not immune")
	}
	rew, ok := sptFirstKillByPetID[1238]
	if !ok || rew.RewardPetID != 1238 {
		t.Fatalf("first kill reward=%v ok=%v", rew, ok)
	}
}

func TestChiWingSkillBlocks(t *testing.T) {
	ohko := &tableloader.SkillDef{ID: 10210, SideEffect: "36", SideEffectArg: "100"}
	if sideEffectOHKOVs(ohko, 80, 1238) != 0 {
		t.Fatal("极度冰点 should be blocked")
	}
	if sideEffectOHKOVs(ohko, 80, 1) != 80 {
		t.Fatal("normal pet takes OHKO")
	}
	pct := &tableloader.SkillDef{ID: 20143, SideEffect: "28", SideEffectArg: "4"}
	if sideEffectPercentHPDamageVs(pct, 100, 1238) != 0 {
		t.Fatal("海洋之心 blocked")
	}
	st := &BattleState{PlayerPetID: 1238, EnemyID: 70}
	doom := &tableloader.SkillDef{ID: 20215, SideEffect: "62", SideEffectArg: "3"}
	armDoom(st, doom, false) // 敌方对玩家施放镇魂歌
	if st.EnemyDoomRounds != 0 {
		t.Fatal("镇魂歌 should not arm on immune pet")
	}
	st.PlayerStages = [6]int8{-2, -1, 0, 0, 1}
	clampPlayerNegativeStages(st)
	if st.PlayerStages[0] != 0 || st.PlayerStages[1] != 0 || st.PlayerStages[4] != 1 {
		t.Fatalf("stages=%v", st.PlayerStages)
	}
}
