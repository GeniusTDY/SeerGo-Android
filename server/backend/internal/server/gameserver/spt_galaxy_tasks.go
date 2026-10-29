package gameserver

import "log"

// 玄武/青龙空间传送阵门禁：客户端 XuanWuController / QingLongController
// 检查 TasksManager 任务完成态，而非 DefeatedSPT。击败对应 SPT 后需补齐这些老任务。

// sptPetToXuanWuTask 帕诺星系 SPT → 玄武任务 ID。
var sptPetToXuanWuTask = map[int]int{
	47:  301, // 蘑菇怪
	34:  302, // 钢牙鲨
	42:  303, // 里奥斯
	69:  305, // 提亚斯
	88:  307, // 纳多雷
	113: 308, // 雷纳多
}

// sptPetToQingLongTask 卡兰星系 SPT → 青龙任务 ID。
var sptPetToQingLongTask = map[int]int{
	264: 312, // 奈尼芬多
	391: 314, // 塔西亚
	274: 315, // 塔克林
	421: 316, // 厄尔塞拉
}

func galaxyPortalTaskIDForBoss(bossPetID int) int {
	if id, ok := sptPetToXuanWuTask[bossPetID]; ok {
		return id
	}
	if id, ok := sptPetToQingLongTask[bossPetID]; ok {
		return id
	}
	return 0
}

// autoCompleteGalaxyPortalTasksIfNeeded 击败 SPT 后补齐传送阵所需任务。
func (s *Server) autoCompleteGalaxyPortalTasksIfNeeded(uid int64, bossPetID int) {
	taskID := galaxyPortalTaskIDForBoss(bossPetID)
	if taskID <= 0 || s.cfg.Store == nil {
		return
	}
	if s.taskAlreadyComplete(uid, taskID) {
		return
	}
	s.setTaskStatus(uid, taskID, taskStatusComplete)
	log.Printf("[spt] portal task complete uid=%d boss=%d task=%d", uid, bossPetID, taskID)
}

// syncGalaxyPortalTasksFromDefeated 登录回填：已击败 SPT 但任务未完成的账号可进玄武/青龙。
func (s *Server) syncGalaxyPortalTasksFromDefeated(uid int64) {
	if s.cfg.Store == nil {
		return
	}
	keys, err := s.cfg.Store.ListDefeatedSPTKeys(uid)
	if err != nil || len(keys) == 0 {
		return
	}
	for _, petID := range keys {
		s.autoCompleteGalaxyPortalTasksIfNeeded(uid, petID)
	}
}
