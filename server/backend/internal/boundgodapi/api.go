package boundgodapi

// 可选战斗扩展钩子；默认关闭。

var IsBoundGodPet = func(petID int) bool { return false }

var ApplyIncomingGuard = func(defenderPetID int, dmg *uint32, crit *bool) {}
