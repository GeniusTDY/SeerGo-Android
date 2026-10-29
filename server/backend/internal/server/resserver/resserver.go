package resserver

import (
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
		// 缓存策略：ruffle 带内容哈希可永久缓存；swf 存缓存但必须回源校验（mtime 变了才重下）；
		// 其余游戏资源给 5 分钟，避免每次进游戏都全量重下。
		setCacheHeaders(w, path)
		if path == "" || strings.HasSuffix(r.URL.Path, "/") {
			rw := &statusWriter{ResponseWriter: w, code: 200}
			files.ServeHTTP(rw, r)
			logRes(r, rw.code, rw.n, "DIR/INDEX", start)
			return
		}

		primary := filepath.Join(nieoDataDir, filepath.FromSlash(path))
		if fi, err := os.Stat(primary); err == nil && fi.Mode().IsRegular() && fi.Size() > 0 {
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
				setCacheHeaders(w, path)
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
func setCacheHeaders(w http.ResponseWriter, path string) {
	low := strings.ToLower(path)
	switch {
	case strings.HasPrefix(low, "ruffle/"):
		// 文件名含内容哈希（ruffle_web_bg.NABW6SMO.wasm），可永久缓存
		w.Header().Set("Cache-Control", "public, max-age=31536000, immutable")
	case strings.HasSuffix(low, ".swf"):
		// 允许落盘缓存，但每次使用前回源校验（Last-Modified 变了才重新下载）
		w.Header().Set("Cache-Control", "no-cache")
	default:
		for _, prefix := range []string{"resource/", "module/", "login/", "flash/", "assets/", "images/", "xml/", "nono/"} {
			if strings.HasPrefix(low, prefix) {
				w.Header().Set("Cache-Control", "public, max-age=300")
				return
			}
		}
	}
}

func EnsureDir(dir string) error {
	return os.MkdirAll(dir, 0o755)
}

func Join(elem ...string) string {
	return filepath.Join(elem...)
}
