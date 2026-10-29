package gameserver

import "math/rand"

// 客户端 status[k] 直接显示剩余回合；对齐参考服未配表时的默认回合。

func defaultStatusRounds(idx int) byte {
	switch idx {
	case statusIdxPara:
		return 2
	case statusIdxTired:
		return 1
	case statusIdxPoison, statusIdxBurn, statusIdxFreeze:
		return byte(rand.Intn(2) + 1) // 1~2
	case statusIdxFear, statusIdxSleep:
		return byte(rand.Intn(3) + 1) // 1~3
	default:
		return 2
	}
}

func tickStatusRounds(st *battleStatus) {
	if st == nil {
		return
	}
	dec := func(v *byte) {
		if *v > 0 {
			*v--
		}
	}
	dec(&st.Para)
	dec(&st.Poison)
	dec(&st.Burn)
	dec(&st.Freeze)
	dec(&st.Fear)
	dec(&st.Tired)
	dec(&st.Sleep)
}

func tickBattleStatusRounds(st *BattleState) {
	if st == nil {
		return
	}
	tickStatusRounds(&st.PlayerStatus)
	tickStatusRounds(&st.EnemyStatus)
}
