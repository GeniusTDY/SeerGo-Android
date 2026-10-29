package resserver

import (
	"compress/gzip"
	"fmt"
	"io"
	"log"
	"net/http"
	"os"
	"path/filepath"
	"strings"
	"time"

	"niaohao/server/internal/conslog"
)

// ListenAndServe 将 NieoData 作为静态资源根目录提供 HTTP 访问。
// 对 dll/NieoCore.swf 等关键文件：若 NieoData 缺失，自动回退到同级 tmp/dll。
func ListenAndServe(addr, nieoDataDir, ipFile string) error {
	mux := http.NewServeMux()
	tmpRoot := filepath.Clean(filepath.Join(nieoDataDir, "..", "tmp"))
	tmpDLL := filepath.Join(tmpRoot, "dll")

	mux.HandleFunc("/ip.txt", func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()
		b, err := os.ReadFile(ipFile)
		if err != nil {
			http.Error(w, "ip.txt missing", http.StatusNotFound)
			logRes(r, 404, 0, "MISS ip.txt", start)
			return
		}
		w.Header().Set("Content-Type", "text/plain; charset=utf-8")
		n, _ := w.Write(b)
		logRes(r, 200, n, "OK configs/ip.txt", start)
	})

	mux.HandleFunc("/wsproxy/", handleWSProxy)

	files := http.FileServer(http.Dir(nieoDataDir))
	mux.HandleFunc("/", func(w http.ResponseWriter, r *http.Request) {
		start := time.Now()
		if r.Method != http.MethodGet && r.Method != http.MethodHead {
			http.Error(w, "method not allowed", http.StatusMethodNotAllowed)
			logRes(r, 405, 0, "METHOD", start)
			return
		}
		path := strings.TrimPrefix(r.URL.Path, "/")
		// 缓存策略：ruffle 带内容哈希可永久缓存；dll 带版本号 query 也可永久缓存；
		// 其余 swf/资源给 5 分钟，避免每次重进游戏都全量重下。
		setCacheHeaders(w, r)
		if path == "" || strings.HasSuffix(r.URL.Path, "/") {
			rw := &statusWriter{ResponseWriter: w, code: 200}
			files.ServeHTTP(rw, r)
			logRes(r, rw.code, rw.n, "DIR/INDEX", start)
			return
		}

		primary := filepath.Join(nieoDataDir, filepath.FromSlash(path))
		if fi, err := os.Stat(primary); err == nil && fi.Mode().IsRegular() && fi.Size() > 0 {
			// ruffle 的 wasm（14MB）/ruffle.js 是大头，按需 gzip 并落缓存，首包体积约降到 1/3
			if wantGzip(r, path, fi.Size()) {
				rw := &statusWriter{ResponseWriter: w, code: 200}
				serveGzipCached(rw, r, primary, filepath.Join(tmpRoot, "gz", filepath.FromSlash(path)+".gz"), path, fi)
				logRes(r, rw.code, rw.n, "OK gzip", start)
				return
			}
			rw := &statusWriter{ResponseWriter: w, code: 200}
			files.ServeHTTP(rw, r)
			logRes(r, rw.code, rw.n, "OK NieoData", start)
			return
		}

		// dll/* → tmp/dll
		if strings.HasPrefix(strings.ToLower(path), "dll/") {
			fallback := filepath.Join(tmpDLL, filepath.Base(path))
			if fi, err := os.Stat(fallback); err == nil && fi.Mode().IsRegular() {
				rw := &statusWriter{ResponseWriter: w, code: 200}
				http.ServeFile(rw, r, fallback)
				logRes(r, rw.code, rw.n, "OK tmp/dll fallback", start)
				return
			}
		}

		// 根目录版本文件等 → tmp/
		base := filepath.Base(path)
		if !strings.Contains(path, "/") {
			for _, cand := range []string{
				filepath.Join(tmpRoot, base),
				filepath.Join(tmpDLL, base),
			} {
				if fi, err := os.Stat(cand); err == nil && fi.Mode().IsRegular() {
					rw := &statusWriter{ResponseWriter: w, code: 200}
					http.ServeFile(rw, r, cand)
					logRes(r, rw.code, rw.n, "OK tmp fallback", start)
					return
				}
			}
		}

		// 本地缺失：先从官服加载并回给客户端，再落盘到 NieoData（下次直接 HIT）。
		if shouldAutoFetch("/" + path) {
			data, src, err := fetchAndCache(nieoDataDir, "/"+path)
			if err == nil && len(data) > 0 {
				setCacheHeaders(w, r)
				w.Header().Set("Content-Type", contentTypeFor(path))
				w.Header().Set("Content-Length", fmt.Sprintf("%d", len(data)))
				if r.Method == http.MethodHead {
					w.WriteHeader(http.StatusOK)
					logRes(r, 200, 0, "FETCH "+src, start)
					return
				}
				n, _ := w.Write(data)
				logRes(r, 200, n, "FETCH "+src, start)
				return
			}
			if err != nil {
				log.Printf("[res] auto-fetch miss %s: %v", path, err)
			}
		}

		rw := &statusWriter{ResponseWriter: w, code: 200}
		files.ServeHTTP(rw, r)
		tag := "OK"
		if rw.code == 404 || rw.code >= 400 {
			tag = "MISS"
		}
		logRes(r, rw.code, rw.n, tag, start)
	})

	fmt.Printf("[res] serving %s on %s (ip.txt -> %s)\n", nieoDataDir, addr, ipFile)
	fmt.Printf("[res] dll fallback: %s\n", tmpDLL)
	fmt.Println("[res] auto-fetch: ON (miss -> seer.61.com / .tw / 51seer → NieoData)")
	fmt.Println("[res] resource request logs: ON ([RES] ...)")
	return http.ListenAndServe(addr, mux)
}

type statusWriter struct {
	http.ResponseWriter
	code int
	n    int
}

func (w *statusWriter) WriteHeader(code int) {
	w.code = code
	w.ResponseWriter.WriteHeader(code)
}

func (w *statusWriter) Write(b []byte) (int, error) {
	if w.code == 0 {
		w.code = 200
	}
	n, err := w.ResponseWriter.Write(b)
	w.n += n
	return n, err
}

// ReadFrom 让文件响应走 net/http 的 sendfile 路径，避免大资源逐块拷进用户态。
func (w *statusWriter) ReadFrom(r io.Reader) (int64, error) {
	if w.code == 0 {
		w.code = 200
	}
	if rf, ok := w.ResponseWriter.(io.ReaderFrom); ok {
		n, err := rf.ReadFrom(r)
		w.n += int(n)
		return n, err
	}
	n, err := io.Copy(writerOnly{w.ResponseWriter}, r)
	w.n += int(n)
	return n, err
}

type writerOnly struct{ io.Writer }

func logRes(r *http.Request, code, bytes int, tag string, start time.Time) {
	path := r.URL.Path
	if r.URL.RawQuery != "" {
		// 截断过长 query（版本号等保留前 48）
		q := r.URL.RawQuery
		if len(q) > 48 {
			q = q[:48] + "..."
		}
		path = path + "?" + q
	}
	ms := time.Since(start).Milliseconds()
	line := fmt.Sprintf("[RES] %3d %-6s %-48s %8dB %4dms  %s  from=%s",
		code, r.Method, trimPath(path, 48), bytes, ms, tag, r.RemoteAddr)
	// 黄字：缺资源 404 / MISS
	if code == 404 || strings.Contains(tag, "MISS") {
		log.Print(conslog.Yellow(line))
		return
	}
	log.Print(line)
}

func trimPath(s string, n int) string {
	if len(s) <= n {
		return s
	}
	return s[:n-3] + "..."
}

// setCacheHeaders 按路径设置缓存策略（原先对所有 swf 设 no-store，导致每次刷新整包重下）。
func setCacheHeaders(w http.ResponseWriter, r *http.Request) {
	low := strings.ToLower(strings.TrimPrefix(r.URL.Path, "/"))
	switch {
	case strings.HasPrefix(low, "ruffle/"):
		// 文件名含内容哈希（ruffle_web_bg.NABW6SMO.wasm），可永久缓存
		w.Header().Set("Cache-Control", "public, max-age=31536000, immutable")
	case strings.HasPrefix(low, "dll/") && strings.HasSuffix(low, ".swf") && r.URL.RawQuery != "":
		// dll.xml 里带 ?版本号，打补丁时版本号会变 → 可长期缓存，重进游戏零请求
		w.Header().Set("Cache-Control", "public, max-age=31536000, immutable")
	case strings.HasSuffix(low, ".swf"):
		// 存本地 5 分钟，过期后回源校验（304），避免每次启动都几十次往返
		w.Header().Set("Cache-Control", "public, max-age=300")
	default:
		for _, prefix := range []string{"resource/", "module/", "login/", "flash/", "assets/", "images/", "xml/", "nono/"} {
			if strings.HasPrefix(low, prefix) {
				w.Header().Set("Cache-Control", "public, max-age=300")
				return
			}
		}
	}
}

// wantGzip 只对 ruffle 的 wasm/js 启用（SWF 本身已是 zlib 压缩，再压没有收益）。
func wantGzip(r *http.Request, path string, size int64) bool {
	if size < 1024 || !strings.Contains(r.Header.Get("Accept-Encoding"), "gzip") {
		return false
	}
	low := strings.ToLower(path)
	if !strings.HasPrefix(low, "ruffle/") {
		return false
	}
	return strings.HasSuffix(low, ".wasm") || strings.HasSuffix(low, ".js")
}

// serveGzipCached 提供 gzip 压缩版本；压缩结果按源文件 mtime 缓存到 tmp/gz/（源文件更新则重新压）。
func serveGzipCached(w http.ResponseWriter, r *http.Request, src, cache, name string, fi os.FileInfo) {
	gz, err := ensureGzip(src, cache, fi)
	if err != nil || gz == nil {
		http.ServeFile(w, r, src) // 压不动就退回原文件
		return
	}
	defer gz.Close()
	gs, err := gz.Stat()
	if err != nil || gs.Size() == 0 {
		http.ServeFile(w, r, src)
		return
	}
	w.Header().Set("Content-Encoding", "gzip")
	w.Header().Set("Vary", "Accept-Encoding")
	if ct := contentTypeFor(name); ct != "application/octet-stream" {
		w.Header().Set("Content-Type", ct)
	}
	// Last-Modified 用源文件 mtime，源文件一变缓存自动失效
	http.ServeContent(w, r, name, fi.ModTime(), gz)
}

func ensureGzip(src, cache string, fi os.FileInfo) (*os.File, error) {
	if gs, err := os.Stat(cache); err == nil && gs.Size() > 0 && !gs.ModTime().Before(fi.ModTime()) {
		return os.Open(cache)
	}
	in, err := os.Open(src)
	if err != nil {
		return nil, err
	}
	defer in.Close()
	if err := os.MkdirAll(filepath.Dir(cache), 0o755); err != nil {
		return nil, err
	}
	tmp := cache + ".tmp"
	out, err := os.Create(tmp)
	if err != nil {
		return nil, err
	}
	zw := gzip.NewWriter(out)
	if _, err := io.Copy(zw, in); err != nil {
		_ = zw.Close()
		_ = out.Close()
		_ = os.Remove(tmp)
		return nil, err
	}
	if err := zw.Close(); err != nil {
		_ = out.Close()
		_ = os.Remove(tmp)
		return nil, err
	}
	if err := out.Close(); err != nil {
		_ = os.Remove(tmp)
		return nil, err
	}
	if err := os.Rename(tmp, cache); err != nil {
		_ = os.Remove(tmp)
		return nil, err
	}
	return os.Open(cache)
}

func EnsureDir(dir string) error {
	return os.MkdirAll(dir, 0o755)
}

func Join(elem ...string) string {
	return filepath.Join(elem...)
}
