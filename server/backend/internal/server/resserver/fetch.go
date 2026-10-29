package resserver

import (
	"bytes"
	"fmt"
	"io"
	"log"
	"net/http"
	"os"
	"path/filepath"
	"strings"
	"sync"
	"time"
)

// 官服资源镜像（与根目录「赛尔资源网址」一致：同路径拼到 host）。
// 大陆站优先，台服/备用站作回退。
var officialBases = []string{
	"https://seer.61.com",
	"http://seer.61.com",
	"https://seer.61.com.tw",
	"http://seer.61.com.tw",
	"https://www.51seer.com",
	"http://www.51seer.com",
}

// flash 区少数资源在 res.61.com（如 freshman.swf）。
var flashBases = []string{
	"https://res.61.com",
	"http://res.61.com",
}

const (
	fetchTimeout   = 45 * time.Second
	maxFetchBytes  = 64 << 20 // 64MB
	minUsefulBytes = 32
)

var (
	fetchClient = &http.Client{Timeout: fetchTimeout}
	inflight    sync.Map // path -> *fetchWait
)

type fetchWait struct {
	done chan struct{}
	data []byte
	err  error
}

// shouldAutoFetch 是否对缺失路径尝试官服拉取（排除配置/目录类）。
func shouldAutoFetch(urlPath string) bool {
	p := strings.TrimPrefix(urlPath, "/")
	if p == "" || strings.HasSuffix(urlPath, "/") {
		return false
	}
	low := strings.ToLower(p)
	switch {
	case low == "ip.txt", low == "ip":
		return false
	case strings.HasPrefix(low, "config/"):
		return false
	case strings.HasPrefix(low, "dll/"):
		// 核心 DLL 禁止官服覆盖（增量包/本地补丁优先）
		return false
	case strings.HasSuffix(low, ".map"), strings.HasSuffix(low, "favicon.ico"):
		return false
	}
	// 游戏资源常见前缀（不含 dll）
	for _, prefix := range []string{
		"resource/", "module/", "login/", "flash/", "xml/",
	} {
		if strings.HasPrefix(low, prefix) {
			return true
		}
	}
	ext := strings.ToLower(filepath.Ext(p))
	switch ext {
	case ".swf", ".mp3", ".png", ".jpg", ".jpeg", ".gif", ".xml", ".bin", ".json":
		return true
	}
	return false
}

// fetchAndCache 从官服拉取资源：先给客户端「加载」用的字节，再写入 NieoData（下载落盘）。
// 同一路径并发请求合并为一次拉取。
func fetchAndCache(nieoDataDir, urlPath string) ([]byte, string, error) {
	urlPath = "/" + strings.TrimPrefix(urlPath, "/")
	if !shouldAutoFetch(urlPath) {
		return nil, "", fmt.Errorf("skip auto-fetch: %s", urlPath)
	}
	if err := guardPath(nieoDataDir, urlPath); err != nil {
		return nil, "", err
	}

	if v, ok := inflight.Load(urlPath); ok {
		w := v.(*fetchWait)
		<-w.done
		return w.data, "WAIT", w.err
	}
	w := &fetchWait{done: make(chan struct{})}
	if actual, loaded := inflight.LoadOrStore(urlPath, w); loaded {
		exist := actual.(*fetchWait)
		<-exist.done
		return exist.data, "WAIT", exist.err
	}
	defer func() {
		close(w.done)
		inflight.Delete(urlPath)
	}()

	data, src, err := downloadOfficial(urlPath)
	w.data, w.err = data, err
	if err != nil {
		return nil, src, err
	}
	savePath := filepath.Join(nieoDataDir, filepath.FromSlash(strings.TrimPrefix(urlPath, "/")))
	if err := saveFileAtomic(savePath, data); err != nil {
		log.Printf("[res] cache save fail %s: %v", savePath, err)
		// 仍返回数据，本次请求可加载成功，下次再试落盘
		return data, src + "+NOSAVE", nil
	}
	return data, src, nil
}

func downloadOfficial(urlPath string) ([]byte, string, error) {
	bases := officialBases
	low := strings.ToLower(urlPath)
	if strings.HasPrefix(low, "/flash/") {
		bases = append(append([]string{}, flashBases...), officialBases...)
	}
	var lastErr error
	for _, base := range bases {
		u := strings.TrimRight(base, "/") + urlPath
		data, err := httpGetBytes(u)
		if err != nil {
			lastErr = err
			continue
		}
		if !looksLikeAsset(urlPath, data) {
			lastErr = fmt.Errorf("invalid body from %s (%d bytes)", base, len(data))
			continue
		}
		return data, base, nil
	}
	if lastErr == nil {
		lastErr = fmt.Errorf("all mirrors miss")
	}
	return nil, "", lastErr
}

func httpGetBytes(u string) ([]byte, error) {
	req, err := http.NewRequest(http.MethodGet, u, nil)
	if err != nil {
		return nil, err
	}
	req.Header.Set("User-Agent", "Mozilla/5.0 (compatible; NieoResCache/1.0)")
	resp, err := fetchClient.Do(req)
	if err != nil {
		return nil, err
	}
	defer resp.Body.Close()
	if resp.StatusCode != http.StatusOK {
		io.Copy(io.Discard, io.LimitReader(resp.Body, 4096))
		return nil, fmt.Errorf("HTTP %d", resp.StatusCode)
	}
	limited := io.LimitReader(resp.Body, maxFetchBytes+1)
	data, err := io.ReadAll(limited)
	if err != nil {
		return nil, err
	}
	if len(data) > maxFetchBytes {
		return nil, fmt.Errorf("too large")
	}
	if len(data) < minUsefulBytes {
		return nil, fmt.Errorf("too small (%d)", len(data))
	}
	return data, nil
}

func looksLikeAsset(urlPath string, data []byte) bool {
	if len(data) < minUsefulBytes {
		return false
	}
	trim := bytes.TrimSpace(data)
	sample := strings.ToLower(string(trim[:min(200, len(trim))]))
	if strings.HasPrefix(sample, "<!doctype") || strings.HasPrefix(sample, "<html") ||
		strings.Contains(sample, "<title>404") {
		return false
	}
	ext := strings.ToLower(filepath.Ext(urlPath))
	switch ext {
	case ".swf":
		return len(data) >= 3 && (bytes.HasPrefix(data, []byte("FWS")) ||
			bytes.HasPrefix(data, []byte("CWS")) || bytes.HasPrefix(data, []byte("ZWS")))
	case ".mp3":
		return bytes.HasPrefix(data, []byte("ID3")) || (data[0] == 0xFF && (data[1]&0xE0) == 0xE0)
	case ".png":
		return bytes.HasPrefix(data, []byte{0x89, 0x50, 0x4E, 0x47})
	case ".jpg", ".jpeg":
		return bytes.HasPrefix(data, []byte{0xFF, 0xD8, 0xFF})
	case ".gif":
		return bytes.HasPrefix(data, []byte("GIF8"))
	default:
		return true
	}
}

func guardPath(nieoDataDir, urlPath string) error {
	rel := strings.TrimPrefix(urlPath, "/")
	if rel == "" || strings.Contains(rel, "..") {
		return fmt.Errorf("bad path")
	}
	absBase, err := filepath.Abs(nieoDataDir)
	if err != nil {
		return err
	}
	absTarget, err := filepath.Abs(filepath.Join(nieoDataDir, filepath.FromSlash(rel)))
	if err != nil {
		return err
	}
	sep := string(os.PathSeparator)
	if absTarget != absBase && !strings.HasPrefix(absTarget, absBase+sep) {
		return fmt.Errorf("path escape")
	}
	return nil
}

func saveFileAtomic(path string, data []byte) error {
	if err := os.MkdirAll(filepath.Dir(path), 0o755); err != nil {
		return err
	}
	tmp := path + ".download"
	if err := os.WriteFile(tmp, data, 0o644); err != nil {
		return err
	}
	if err := os.Rename(tmp, path); err != nil {
		_ = os.Remove(path)
		if err2 := os.Rename(tmp, path); err2 != nil {
			_ = os.Remove(tmp)
			return err2
		}
	}
	return nil
}

func min(a, b int) int {
	if a < b {
		return a
	}
	return b
}

func contentTypeFor(path string) string {
	switch strings.ToLower(filepath.Ext(path)) {
	case ".swf":
		return "application/x-shockwave-flash"
	case ".mp3":
		return "audio/mpeg"
	case ".png":
		return "image/png"
	case ".jpg", ".jpeg":
		return "image/jpeg"
	case ".gif":
		return "image/gif"
	case ".xml":
		return "application/xml"
	case ".js":
		return "application/javascript"
	case ".json":
		return "application/json"
	case ".txt":
		return "text/plain; charset=utf-8"
	default:
		return "application/octet-stream"
	}
}
