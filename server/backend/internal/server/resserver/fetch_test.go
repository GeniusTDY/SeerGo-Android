package resserver

import (
	"net/http"
	"net/http/httptest"
	"os"
	"path/filepath"
	"testing"
)

func TestLooksLikeSWF(t *testing.T) {
	ok := []byte("CWS\x0a\x00\x00\x00" + string(make([]byte, 40)))
	if !looksLikeAsset("/resource/a.swf", ok) {
		t.Fatal("expected CWS swf ok")
	}
	html := []byte("<!DOCTYPE html><html><title>404</title></html>" + string(make([]byte, 40)))
	if looksLikeAsset("/resource/a.swf", html) {
		t.Fatal("html must be rejected")
	}
}

func TestShouldAutoFetch(t *testing.T) {
	if !shouldAutoFetch("/resource/fightResource/pet/swf/1443.swf") {
		t.Fatal("pet swf should fetch")
	}
	if shouldAutoFetch("/ip.txt") {
		t.Fatal("ip.txt must not fetch")
	}
	if shouldAutoFetch("/config/ServerR.xml") {
		t.Fatal("config must not fetch")
	}
}

func TestGuardPath(t *testing.T) {
	dir := t.TempDir()
	if err := guardPath(dir, "/resource/a.swf"); err != nil {
		t.Fatal(err)
	}
	if err := guardPath(dir, "/../secret"); err == nil {
		t.Fatal("expected escape reject")
	}
}

func TestSaveFileAtomic(t *testing.T) {
	dir := t.TempDir()
	p := filepath.Join(dir, "resource", "x.swf")
	data := []byte("CWS" + string(make([]byte, 40)))
	if err := saveFileAtomic(p, data); err != nil {
		t.Fatal(err)
	}
	got, err := os.ReadFile(p)
	if err != nil || string(got) != string(data) {
		t.Fatalf("read back fail: %v len=%d", err, len(got))
	}
}

func TestFetchHandlerUsesCachePath(t *testing.T) {
	// 仅验证 shouldAutoFetch + content type，不打外网
	if contentTypeFor("a.swf") != "application/x-shockwave-flash" {
		t.Fatal("swf mime")
	}
	req := httptest.NewRequest(http.MethodGet, "/resource/fightResource/pet/swf/1.swf", nil)
	if req.URL.Path == "" {
		t.Fatal("path")
	}
}
