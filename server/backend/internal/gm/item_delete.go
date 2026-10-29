package gm

import (
	"net/http"
)

// handleDeleteItem POST /api/item/delete
// { uid|userId, itemId, count?, confirm:true }
// count 省略或 <=0：删除该道具全部数量；count>0：扣除指定数量（不超过持有量）。
func (s *Server) handleDeleteItem(w http.ResponseWriter, r *http.Request) {
	if r.Method != http.MethodPost {
		writeErr(w, 405, "POST only")
		return
	}
	var req struct {
		UserID  int64  `json:"userId"`
		UID     int64  `json:"uid"`
		ItemID  int    `json:"itemId"`
		Count   int    `json:"count"`
		Confirm bool   `json:"confirm"`
		Admin   string `json:"admin"`
	}
	if err := decodeJSON(r, &req); err != nil {
		writeErr(w, 400, err.Error())
		return
	}
	if !req.Confirm {
		writeErr(w, 400, "需要 confirm=true")
		return
	}
	uid := req.UserID
	if uid <= 0 {
		uid = req.UID
	}
	if s.cfg.Store == nil || uid <= 0 || req.ItemID <= 0 {
		writeErr(w, 400, "需要 userId/itemId")
		return
	}
	u, err := s.cfg.Store.FindByUserID(uid)
	if err != nil || u == nil {
		writeErr(w, 404, "用户不存在")
		return
	}
	have, err := s.cfg.Store.GetItemCount(uid, req.ItemID)
	if err != nil {
		writeErr(w, 500, err.Error())
		return
	}
	if have <= 0 {
		writeErr(w, 404, "用户未拥有该物品")
		return
	}
	remove := have
	if req.Count > 0 && req.Count < have {
		remove = req.Count
	}
	if err := s.cfg.Store.ConsumeItem(uid, req.ItemID, remove); err != nil {
		writeErr(w, 500, err.Error())
		return
	}
	left := have - remove
	admin := actorOrHeader(r, req.Admin)
	label := s.itemLabel(req.ItemID)
	aid, _ := s.cfg.Store.InsertGMAudit(admin, "item_delete", uid, map[string]any{
		"itemId": req.ItemID, "label": label, "removed": remove, "before": have, "left": left,
	})
	writeJSON(w, map[string]any{
		"ok": true, "auditId": aid, "label": label,
		"itemId": req.ItemID, "removed": remove, "left": left,
	})
}
