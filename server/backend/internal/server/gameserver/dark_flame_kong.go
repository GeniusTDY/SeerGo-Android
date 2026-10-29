package gameserver

import "niaohao/server/internal/boundgodapi"

func isBoundGodPet(petID int) bool {
	return boundgodapi.IsBoundGodPet(petID)
}

func playerHasInfinitePP(st *BattleState) bool {
	return st != nil && isBoundGodPet(st.PlayerPetID)
}

func forcePlayerAlwaysFirst(st *BattleState) bool {
	return st != nil && isBoundGodPet(st.PlayerPetID)
}

func foeSkillsNullifiedAgainst(defenderPetID int) bool {
	return isBoundGodPet(defenderPetID)
}

func enemyOffenseNullified(st *BattleState) bool {
	return st != nil && foeSkillsNullifiedAgainst(st.PlayerPetID)
}

func isDamageImmunePet(petID int) bool {
	return isBoundGodPet(petID)
}

func isReflectImmunePet(petID int) bool {
	return isBoundGodPet(petID)
}

func isDrainImmunePet(petID int) bool {
	return isBoundGodPet(petID)
}

func applyBoundGodPetIncomingGuard(defenderPetID int, dmg *uint32, crit *bool) {
	boundgodapi.ApplyIncomingGuard(defenderPetID, dmg, crit)
}
