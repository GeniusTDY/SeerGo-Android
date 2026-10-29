package gameserver

import (
	"math/rand"

	"niaohao/server/internal/tableloader"
)

// 炫彩山迪符特 SPT（地图 450）。卡修斯特训若日后复用 pet 804，须按地图隔离，勿走本机制。
const (
	mapIDXuancaiFoot  = 445
	mapIDDifuteSPT    = 450
	petIDKaxiusi      = 798
	petIDDifute       = 804
	difuteSPTEvadePct = 30
)

func isDifuteSPTBossBattle(st *BattleState) bool {
	return st != nil && st.EnemyID == petIDDifute && st.MapID == mapIDDifuteSPT
}

// applyDifuteSPTEvade 非必中技能额外 30% miss（对照原服 handleFightSkill）。
func applyDifuteSPTEvade(st *BattleState, d *tableloader.SkillDef, playerHit bool) bool {
	if !playerHit || !isDifuteSPTBossBattle(st) {
		return playerHit
	}
	if skillIsMustHit(d) || mustHitFromBuff(&st.PlayerBuff) {
		return true
	}
	if rand.Intn(100) < difuteSPTEvadePct {
		return false
	}
	return true
}
