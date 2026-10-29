package gm

import (
	"net/http"
	"time"
)

const gmSuperNonoMaxLevel = 15

// handleNonoSet POST /api/nono/set
// { userId, superLevel, vipDays? } — 设置超能 NoNo 等级；vipDays 默认 3650（约 10 年）。
func (s *Server) handleNonoSet(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		writeErr(w, 405, "POST only")
		return
	}
	if s.cfg.Store == nil {
		writeErr(w, 500, "store 未就绪")
		return
	}
	var req struct {
		UserID     int64 `json:"userId"`
		SuperLevel int   `json:"superLevel"`
		VipDays    int   `json:"vipDays"`
		Admin      string `json:"admin"`
	}
	if err := decodeJSON(r, &req); err != nil {
		writeErr(w, 400, err.Error())
		return
	}
	if req.UserID <= 0 {
		writeErr(w, 400, "userId 无效")
		return
	}
	if req.SuperLevel < 0 {
		req.SuperLevel = 0
	}
	if req.SuperLevel > gmSuperNonoMaxLevel {
		req.SuperLevel = gmSuperNonoMaxLevel
	}
	if req.VipDays <= 0 {
		req.VipDays = 3650
	}
	if req.VipDays > 36500 {
		req.VipDays = 36500
	}

	n, err := s.cfg.Store.GetOrInitNono(req.UserID)
	if err != nil || n == nil {
		writeErr(w, 500, "读取 NoNo 失败")
		return
	}
	n.UserID = req.UserID
	n.HasNono = 1
	if n.Flag == 0 {
		n.Flag = 1
	}
	if n.Nick == "" {
		n.Nick = "NoNo"
	}
	n.SuperLevel = req.SuperLevel
	if req.SuperLevel > 0 {
		if n.SuperMonths < req.SuperLevel {
			n.SuperMonths = req.SuperLevel
		}
		n.VipEndTime = time.Now().AddDate(0, 0, req.VipDays).Unix()
		n.SuperStage = gmSuperStageByLevel(req.SuperLevel)
		if n.SuperNono != 6 { // 保留至尊外观码
			n.SuperNono = n.SuperStage
			if n.SuperNono < 1 {
				n.SuperNono = 1
			}
		}
	} else {
		n.SuperStage = 0
		if n.SuperNono != 6 {
			n.SuperNono = 0
		}
		n.VipEndTime = 0
	}
	if err := s.cfg.Store.UpsertNono(n); err != nil {
		writeErr(w, 500, err.Error())
		return
	}
	admin := req.Admin
	if admin == "" {
		admin = r.Header.Get("X-GM-User")
	}
	auditID, _ := s.cfg.Store.InsertGMAudit(admin, "nono.set", req.UserID, map[string]any{
		"superLevel": n.SuperLevel, "superStage": n.SuperStage, "superMonths": n.SuperMonths,
		"vipEndTime": n.VipEndTime, "vipDays": req.VipDays,
	})
	writeJSON(w, map[string]any{
		"ok": true, "auditId": auditID,
		"userId": req.UserID,
		"hasNono": n.HasNono, "superLevel": n.SuperLevel, "superStage": n.SuperStage,
		"superMonths": n.SuperMonths, "superNono": n.SuperNono, "vipEndTime": n.VipEndTime,
	})
}

func gmSuperStageByLevel(level int) int {
	if level < 1 {
		return 0
	}
	switch {
	case level >= 12:
		return 5
	case level >= 9:
		return 4
	case level >= 7:
		return 3
	case level >= 4:
		return 2
	default:
		return 1
	}
}
