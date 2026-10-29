package gm

import (
	"encoding/json"
	"net/http"
	"net/http/httptest"
	"path/filepath"
	"strconv"
	"strings"
	"testing"

	"niaohao/server/internal/store"
)

func newItemDeleteTestServer(t *testing.T) (*Server, *store.MySQL, int64) {
	t.Helper()
	db, err := store.OpenJSON(filepath.Join(t.TempDir(), "saves"))
	if err != nil {
		t.Fatal(err)
	}
	t.Cleanup(func() { _ = db.Close() })
	u, err := db.CreateUser("item-del@local", "abcd")
	if err != nil {
		t.Fatal(err)
	}
	if err := db.AddItem(u.UserID, 300001, 5); err != nil {
		t.Fatal(err)
	}
	return &Server{cfg: Config{Store: db}}, db, u.UserID
}

func postDeleteItem(s *Server, body string) *httptest.ResponseRecorder {
	r := httptest.NewRequest(http.MethodPost, "/api/item/delete", strings.NewReader(body))
	r.Header.Set("Content-Type", "application/json")
	w := httptest.NewRecorder()
	s.handleDeleteItem(w, r)
	return w
}

func TestHandleDeleteItem_FullDelete(t *testing.T) {
	s, db, uid := newItemDeleteTestServer(t)
	w := postDeleteItem(s, `{"userId":`+strconv.FormatInt(uid, 10)+`,"itemId":300001,"confirm":true}`)
	if w.Code != 200 {
		t.Fatalf("status=%d body=%s", w.Code, w.Body.String())
	}
	var resp map[string]any
	if err := json.Unmarshal(w.Body.Bytes(), &resp); err != nil {
		t.Fatal(err)
	}
	if resp["ok"] != true {
		t.Fatalf("resp=%v", resp)
	}
	if n, _ := db.GetItemCount(uid, 300001); n != 0 {
		t.Fatalf("left=%d", n)
	}
}

func TestHandleDeleteItem_UIDField(t *testing.T) {
	s, db, uid := newItemDeleteTestServer(t)
	w := postDeleteItem(s, `{"uid":`+strconv.FormatInt(uid, 10)+`,"itemId":300001,"confirm":true}`)
	if w.Code != 200 {
		t.Fatalf("status=%d body=%s", w.Code, w.Body.String())
	}
	if n, _ := db.GetItemCount(uid, 300001); n != 0 {
		t.Fatalf("left=%d", n)
	}
}

func TestHandleDeleteItem_PartialCount(t *testing.T) {
	s, db, uid := newItemDeleteTestServer(t)
	w := postDeleteItem(s, `{"userId":`+strconv.FormatInt(uid, 10)+`,"itemId":300001,"count":2,"confirm":true}`)
	if w.Code != 200 {
		t.Fatalf("status=%d body=%s", w.Code, w.Body.String())
	}
	if n, _ := db.GetItemCount(uid, 300001); n != 3 {
		t.Fatalf("left=%d", n)
	}
}

func TestHandleDeleteItem_NeedConfirm(t *testing.T) {
	s, _, uid := newItemDeleteTestServer(t)
	w := postDeleteItem(s, `{"userId":`+strconv.FormatInt(uid, 10)+`,"itemId":300001}`)
	if w.Code != 400 {
		t.Fatalf("status=%d body=%s", w.Code, w.Body.String())
	}
}

func TestHandleDeleteItem_NotOwned(t *testing.T) {
	s, _, uid := newItemDeleteTestServer(t)
	w := postDeleteItem(s, `{"userId":`+strconv.FormatInt(uid, 10)+`,"itemId":999999,"confirm":true}`)
	if w.Code != 404 {
		t.Fatalf("status=%d body=%s", w.Code, w.Body.String())
	}
}

func TestHandleDeleteItem_RouteRegistered(t *testing.T) {
	s := New(Config{StaticDir: t.TempDir()})
	r := httptest.NewRequest(http.MethodPost, "/api/item/delete", strings.NewReader(`{"confirm":true}`))
	w := httptest.NewRecorder()
	s.Mux.ServeHTTP(w, r)
	if w.Code == http.StatusNotFound {
		t.Fatalf("route not registered: %s", w.Body.String())
	}
}
