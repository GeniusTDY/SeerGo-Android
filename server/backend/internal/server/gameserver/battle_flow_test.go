package gameserver

import (
	"encoding/binary"
	"testing"
)

func TestBuildAttackValueSize(t *testing.T) {
	av := buildAttackValue(10002, 10004, 1, 10, 0, 190, 200, 0, 0, 0, nil)
	if len(av) != 98 {
		t.Fatalf("empty skills len=%d want 98", len(av))
	}
	skills := [][2]uint32{{10004, 34}, {20002, 40}}
	av2 := buildAttackValue(10002, 10004, 1, 10, 0, 190, 200, 0, 1, 0, skills)
	if len(av2) != 98+16 {
		t.Fatalf("with 2 skills len=%d want %d", len(av2), 98+16)
	}
	n := binary.BigEndian.Uint32(av2[36:40]) // skillCnt after petStatus@32
	if n != 2 {
		t.Fatalf("skillListCount=%d", n)
	}
}

func TestBuildAttackValueLevelField(t *testing.T) {
	st := &BattleState{PlayerLevel: 87, EnemyLevel: 42}
	pav := buildAttackValueFromState(1, 10001, 1, 10, 0, 100, 100, 0, 0, 0, st, true, nil)
	// trailing: typeRelation + skillTypeRelation + priorityEffect; level is 4th u32 from end
	if got := binary.BigEndian.Uint32(pav[len(pav)-16 : len(pav)-12]); got != 87 {
		t.Fatalf("player level=%d want 87", got)
	}
	eav := buildAttackValueFromState(0, 10001, 1, 10, 0, 50, 100, 0, 0, 0, st, false, nil)
	if got := binary.BigEndian.Uint32(eav[len(eav)-16 : len(eav)-12]); got != 42 {
		t.Fatalf("enemy level=%d want 42", got)
	}
}

func TestBuildAttackValueStatusBattleLv(t *testing.T) {
	st := &BattleState{}
	st.PlayerStatus.Para = 2
	st.PlayerStages[stageAtk] = 2
	st.EnemyStatus.Burn = 2
	st.EnemyStages[stageDef] = -1
	pav := buildAttackValueFromState(1, 10001, 1, 10, 0, 100, 100, 0, 0, 0, st, true, nil)
	statusOff := 44 // after isCrit at 40 when skillCount=0 (petStatus@32)
	if pav[statusOff] != 2 {
		t.Fatalf("player para status byte=%d", pav[statusOff])
	}
	if int8(pav[statusOff+20]) != 2 {
		t.Fatalf("player atk stage=%d", int8(pav[statusOff+20]))
	}
	eav := buildAttackValueFromState(0, 10001, 1, 10, 0, 50, 100, 0, 0, 0, st, false, nil)
	if eav[statusOff+2] != 2 {
		t.Fatalf("enemy burn status=%d", eav[statusOff+2])
	}
	if int8(eav[statusOff+21]) != -1 {
		t.Fatalf("enemy def stage=%d", int8(eav[statusOff+21]))
	}
}

func TestEncodeParasiteSeedStatus(t *testing.T) {
	st := &BattleState{}
	st.EnemyBuff.DrainRounds = 5
	ps := encodeFightStatusForSide(st, true)
	if ps[statusIdxDrain] != 5 {
		t.Fatalf("player drain icon=%d want 5", ps[statusIdxDrain])
	}
	es := encodeFightStatusForSide(st, false)
	if es[statusIdxDrained] != 5 {
		t.Fatalf("enemy drained icon=%d want 5", es[statusIdxDrained])
	}
	st.PlayerBuff.ImmuneStatusRounds = 3
	ps = encodeFightStatusForSide(st, true)
	if ps[statusIdxImmuneStatus] != 3 {
		t.Fatalf("immune status icon=%d want 3", ps[statusIdxImmuneStatus])
	}
}

func TestNewlyControlledAfterOpponent(t *testing.T) {
	at := snapshotControlStatus(battleStatus{})
	if !newlyControlledAfterOpponent(battleStatus{Sleep: 1}, at) {
		t.Fatal("new sleep should block")
	}
	if newlyControlledAfterOpponent(battleStatus{}, at) {
		t.Fatal("no status should not block")
	}
	at2 := snapshotControlStatus(battleStatus{Sleep: 1})
	if newlyControlledAfterOpponent(battleStatus{Sleep: 1}, at2) {
		t.Fatal("preexisting sleep is not newly controlled")
	}
}

func TestStatusRoundsTickDown(t *testing.T) {
	st := &BattleState{}
	st.EnemyStatus.Burn = 2
	st.EnemyStatus.Sleep = 1
	out := encodeFightStatus(st.EnemyStatus)
	if out[statusIdxBurn] != 2 || out[statusIdxSleep] != 1 {
		t.Fatalf("encode burn=%d sleep=%d", out[statusIdxBurn], out[statusIdxSleep])
	}
	tickStatusRounds(&st.EnemyStatus)
	if st.EnemyStatus.Burn != 1 || st.EnemyStatus.Sleep != 0 {
		t.Fatalf("after tick burn=%d sleep=%d", st.EnemyStatus.Burn, st.EnemyStatus.Sleep)
	}
	tickStatusRounds(&st.EnemyStatus)
	if st.EnemyStatus.Burn != 0 {
		t.Fatalf("burn should clear, got %d", st.EnemyStatus.Burn)
	}
	out2 := encodeFightStatus(st.EnemyStatus)
	if out2[statusIdxBurn] != 0 || out2[statusIdxSleep] != 0 {
		t.Fatal("icons should be 0 when cleared")
	}
}

func TestAttackValueIncludesParasiteBeforeDecrement(t *testing.T) {
	st := &BattleState{PlayerHP: 100, PlayerMaxHP: 100, EnemyHP: 100, EnemyMaxHP: 100}
	st.EnemyBuff.DrainRounds = 5
	tickBattleBuffEffects(st)
	av := buildAttackValueFromState(1, 10001, 1, 10, 0, int32(st.PlayerHP), st.PlayerMaxHP, 0, 0, 0, st, true, nil)
	statusOff := 44
	if av[statusOff+statusIdxDrain] != 5 {
		t.Fatalf("drain rounds in AV=%d want 5 (before decrement)", av[statusOff+statusIdxDrain])
	}
	decrementBattleBuffRounds(st)
	if st.EnemyBuff.DrainRounds != 4 {
		t.Fatalf("after dec DrainRounds=%d want 4", st.EnemyBuff.DrainRounds)
	}
}

func TestAttackValueRemainIsAttackerOwnHP(t *testing.T) {
	const uid uint32 = 10001
	playerHP, playerMax := uint32(80), uint32(100)
	enemyHP, enemyMax := uint32(0), uint32(200)
	playerAv := buildAttackValue(uid, 10001, 1, 50, 0, int32(playerHP), playerMax, 0, 0, 0, nil)
	enemyAv := buildAttackValue(0, 0, 0, 0, 0, int32(enemyHP), enemyMax, 0, 0, 0, nil)

	if got := binary.BigEndian.Uint32(playerAv[20:24]); got != playerHP {
		t.Fatalf("player remain=%d want %d", got, playerHP)
	}
	if got := binary.BigEndian.Uint32(playerAv[24:28]); got != playerMax {
		t.Fatalf("player max=%d want %d", got, playerMax)
	}
	if got := int32(binary.BigEndian.Uint32(enemyAv[20:24])); got != 0 {
		t.Fatalf("enemy remain=%d want 0 (npcIsdied)", got)
	}
	if binary.BigEndian.Uint32(enemyAv[0:4]) != 0 {
		t.Fatal("enemy userID must be 0")
	}
}

func TestAttackValueLostHPIsFormulaDamage(t *testing.T) {
	const formula uint32 = 80
	av := buildAttackValue(10001, 10001, 1, formula, 0, 150, 200, 0, 0, 0, nil)
	if got := binary.BigEndian.Uint32(av[12:16]); got != formula {
		t.Fatalf("lostHP=%d want formula %d", got, formula)
	}
}

func TestBuildChangePetInfo(t *testing.T) {
	b := buildChangePetInfo(10002, 4, "伊优", 5, 21, 21, 1768449796)
	if len(b) != 44 {
		t.Fatalf("len=%d want 44", len(b))
	}
	if binary.BigEndian.Uint32(b[4:8]) != 4 {
		t.Fatal("petID")
	}
	if binary.BigEndian.Uint32(b[40:44]) != 0 {
		t.Fatal("default typeFlags")
	}
}

func TestBuildNoteStartFight(t *testing.T) {
	player := buildFightPetInfo(1, 10, "a", 99, 100, 100, 10, 0, [6]int8{})
	enemy := buildFightPetInfo(0, 70, "b", 0, 200, 200, 70, 0, [6]int8{2})
	body := buildNoteStartFight(0, player, enemy, packTypeRelationFlags(1, 2))
	if len(body) != 108 {
		t.Fatalf("len=%d want 108", len(body))
	}
	if binary.BigEndian.Uint32(body[104:108]) != packTypeRelationFlags(1, 2) {
		t.Fatal("typeFlags")
	}
}

func TestBuildNoteUpdateProp(t *testing.T) {
	b := buildNoteUpdateProp(1768449796, 4, 5, 10, 10, 50, 21, 30, 30, 30, 30, 30, [6]int{})
	if len(b) != 80 {
		t.Fatalf("len=%d want 80", len(b))
	}
}

func TestPotionTables(t *testing.T) {
	if potionHealHP(300011) != 20 || potionRestorePP(300016) != 5 {
		t.Fatal("potion map")
	}
}
