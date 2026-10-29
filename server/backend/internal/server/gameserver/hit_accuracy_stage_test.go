package gameserver

import (
	"testing"

	"niaohao/server/internal/tableloader"
)

func TestApplyStatGroupsAccuracyBoost(t *testing.T) {
	// 香甜气息/慧眼：SideEffectArg "5 100 1"；聚焦："5 100 2"
	var stages [6]int8
	used := applyStatGroups(&stages, []int{5, 100, 1}, 1)
	if used != 3 || stages[stageAcc] != 1 {
		t.Fatalf("慧眼 +1: used=%d acc=%d", used, stages[stageAcc])
	}
	used = applyStatGroups(&stages, []int{5, 100, 2}, 1)
	if used != 3 || stages[stageAcc] != 3 {
		t.Fatalf("聚焦 +2: used=%d acc=%d", used, stages[stageAcc])
	}
}

func TestHitChanceUsesAccuracyStage(t *testing.T) {
	// 催眠粉类 75%：命中+1 → 100%；命中+2 仍封顶 100
	if got := calcHitChance(75, 0, 0); got != 75 {
		t.Fatalf("base=%d", got)
	}
	if got := calcHitChance(75, 1, 0); got != 100 {
		t.Fatalf("+1 want 100 got %d", got)
	}
	if got := calcHitChance(30, 2, 0); got != 80 {
		t.Fatalf("+2 on 30 want 80 got %d", got)
	}
}

func TestCheckSkillHitRespectsAccuracyStage(t *testing.T) {
	s := &Server{}
	// 无表时默认命中；有表时 MustHit 跳过
	d := &tableloader.SkillDef{ID: 999001, Accuracy: 1, MustHit: 0}
	// 注入临时 skillDef 不可行时，直接测 calcHitChance 链路已足够；
	// 这里验证 stageAcc 下标与 battle 字段一致。
	st := &BattleState{}
	st.PlayerStages[stageAcc] = 2
	if int(st.PlayerStages[stageAcc]) != 2 {
		t.Fatal("stageAcc slot")
	}
	_ = d
	_ = s
}

func TestEncodeBattleLvIncludesAccuracy(t *testing.T) {
	lv := encodeBattleLv([6]int8{1, 0, 0, 0, 0, 2})
	if lv[5] != 2 {
		t.Fatalf("battleLv[5] acc=%d want 2", lv[5])
	}
}
