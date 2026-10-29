package gameserver

import (
	"testing"

	"niaohao/server/internal/tableloader"
)

func TestIsDifuteSPTBossBattle(t *testing.T) {
	if !isDifuteSPTBossBattle(&BattleState{EnemyID: petIDDifute, MapID: mapIDDifuteSPT}) {
		t.Fatal("map450 迪符特应为炫彩山 SPT")
	}
	if isDifuteSPTBossBattle(&BattleState{EnemyID: petIDDifute, MapID: mapIDXuancaiFoot}) {
		t.Fatal("卡修斯图上的 804 不应走迪符特 SPT 闪避")
	}
	if isDifuteSPTBossBattle(&BattleState{EnemyID: petIDKaxiusi, MapID: mapIDXuancaiFoot}) {
		t.Fatal("卡修斯不应走迪符特 SPT 闪避")
	}
}

func TestApplyDifuteSPTEvadeMustHit(t *testing.T) {
	st := &BattleState{EnemyID: petIDDifute, MapID: mapIDDifuteSPT}
	must := &tableloader.SkillDef{MustHit: 1, Accuracy: 1}
	if !applyDifuteSPTEvade(st, must, true) {
		t.Fatal("必中技能不应被迪符特 30% 闪避")
	}
	st.PlayerBuff.MustHitRounds = 1
	normal := &tableloader.SkillDef{MustHit: 0, Accuracy: 100}
	if !applyDifuteSPTEvade(st, normal, true) {
		t.Fatal("必中 buff 不应被迪符特 30% 闪避")
	}
}

func TestApplyDifuteSPTEvadeOnlyMap450(t *testing.T) {
	normal := &tableloader.SkillDef{MustHit: 0, Accuracy: 100}
	st := &BattleState{EnemyID: petIDKaxiusi, MapID: mapIDXuancaiFoot}
	for i := 0; i < 40; i++ {
		if !applyDifuteSPTEvade(st, normal, true) {
			t.Fatal("卡修斯战不应额外闪避")
		}
	}
}

func TestApplyDifuteSPTEvadeRate(t *testing.T) {
	st := &BattleState{EnemyID: petIDDifute, MapID: mapIDDifuteSPT}
	d := &tableloader.SkillDef{MustHit: 0, Accuracy: 100}
	miss := 0
	const n = 400
	for i := 0; i < n; i++ {
		if !applyDifuteSPTEvade(st, d, true) {
			miss++
		}
	}
	if miss == 0 || miss == n {
		t.Fatalf("迪符特额外闪避应介于 0 与 %d 之间，got %d", n, miss)
	}
}
