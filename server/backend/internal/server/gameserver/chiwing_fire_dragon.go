package gameserver

// 炽翼火龙（火山星洞穴 map16 / param2=1；PetID=1238）
// Boss：Lv100、HP10000、双攻500；任意出招使对方烧伤。
// 首杀：发放同名精灵，永久免疫异常、能力下降，以及同生共死/极度冰点/镇魂歌/海洋之心。

const petIDChiWingFireDragon = 1238

var chiWingImmuneSkillIDs = map[uint32]bool{
	10036: true, // 同生共死
	10210: true, // 极度冰点
	20215: true, // 镇魂歌
	20143: true, // 海洋之心
}

func isChiWingFireDragon(petID int) bool {
	return petID == petIDChiWingFireDragon
}

// isStatusImmunePet 玩家出战宠永久免疫异常（首杀炽翼火龙、绑定暗黑炽焰金刚等）。
func isStatusImmunePet(petID int) bool {
	return isChiWingFireDragon(petID) || isBoundGodPet(petID)
}

// isStatDropImmunePet 玩家出战宠永久免疫能力下降。
func isStatDropImmunePet(petID int) bool {
	return isChiWingFireDragon(petID) || isBoundGodPet(petID)
}

// petImmuneToSkill 防守方精灵免疫指定招式（按技能 ID）。
// 绑定神宠：免疫全部招式（含秒杀/百分比粉伤/延迟秒杀等按技能判定的效果）。
func petImmuneToSkill(defenderPetID int, skillID uint32) bool {
	if defenderPetID <= 0 {
		return false
	}
	if isBoundGodPet(defenderPetID) {
		return true
	}
	if skillID == 0 {
		return false
	}
	if !isChiWingFireDragon(defenderPetID) {
		return false
	}
	return chiWingImmuneSkillIDs[skillID]
}

func applyBossFixedStats(st *BattleState) {
	if st == nil || st.isPvP() {
		return
	}
	if isChiWingFireDragon(st.EnemyID) {
		st.EnemyAtk = 500
		st.EnemySpAtk = 500
	}
}

// applyChiWingBossBurn 炽翼火龙本回合出招 → 对方烧伤（可被免疫异常挡住）。
func applyChiWingBossBurn(st *BattleState, enemyUsedSkill bool) {
	if st == nil || !enemyUsedSkill || !isChiWingFireDragon(st.EnemyID) {
		return
	}
	if isStatusImmunePet(st.PlayerPetID) || statusImmuneFromBuff(&st.PlayerBuff) {
		return
	}
	st.PlayerStatus.Burn = defaultStatusRounds(statusIdxBurn)
}

// clampPlayerNegativeStages 玩家宠免疫能力下降时抬回 0。
func clampPlayerNegativeStages(st *BattleState) {
	if st == nil || !isStatDropImmunePet(st.PlayerPetID) {
		return
	}
	for i := range st.PlayerStages {
		if st.PlayerStages[i] < 0 {
			st.PlayerStages[i] = 0
		}
	}
}
