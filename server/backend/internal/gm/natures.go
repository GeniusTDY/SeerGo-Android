package gm

import (
	"net/http"
	"strconv"
)

// 性格：ID + 中文名（与客户端一致，供 GM 下拉）。
var natureCatalog = []struct {
	ID   int
	Name string
}{
	{0, "孤独"}, {1, "勇敢"}, {2, "固执"}, {3, "调皮"},
	{4, "胆小"}, {5, "急躁"}, {6, "开朗"}, {7, "天真"},
	{8, "大胆"}, {9, "悠闲"}, {10, "顽皮"}, {11, "无虑"},
	{12, "保守"}, {13, "稳重"}, {14, "冷静"}, {15, "马虎"},
	{16, "沉着"}, {17, "温顺"}, {18, "狂妄"}, {19, "慎重"},
	{20, "害羞"}, {21, "浮躁"}, {22, "坦率"}, {23, "实干"}, {24, "认真"},
}

func (s *Server) handleNatures(w http.ResponseWriter, _ *http.Request) {
	list := make([]map[string]any, 0, len(natureCatalog))
	for _, n := range natureCatalog {
		list = append(list, map[string]any{
			"id": n.ID, "name": n.Name,
			"label": n.Name + "(" + strconv.Itoa(n.ID) + ")",
		})
	}
	writeJSON(w, map[string]any{"ok": true, "list": list})
}
