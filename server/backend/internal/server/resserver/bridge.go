package resserver

import (
	"crypto/sha1"
	"encoding/base64"
	"encoding/binary"
	"fmt"
	"io"
	"log"
	"net"
	"net/http"
	"strings"
	"time"
)

// 允许通过 /wsproxy/<port> 桥接的目标端口（仅本机游戏服端口）。
var wsProxyTargets = map[string]bool{
	"22973": true, // 登录 TCP
	"28610": true, // 游戏 TCP
}

// handleWSProxy 把浏览器 WebSocket 帧透传到本机 TCP 端口（Ruffle socketProxy）。
func handleWSProxy(w http.ResponseWriter, r *http.Request) {
	port := strings.TrimPrefix(r.URL.Path, "/wsproxy/")
	port = strings.Trim(port, "/")
	if !wsProxyTargets[port] {
		http.Error(w, "forbidden target", http.StatusForbidden)
		return
	}
	if !strings.EqualFold(r.Header.Get("Upgrade"), "websocket") {
		http.Error(w, "websocket only", http.StatusBadRequest)
		return
	}
	key := r.Header.Get("Sec-WebSocket-Key")
	if key == "" {
		http.Error(w, "missing key", http.StatusBadRequest)
		return
	}
	hj, ok := w.(http.Hijacker)
	if !ok {
		http.Error(w, "hijack unsupported", http.StatusInternalServerError)
		return
	}
	client, rw, err := hj.Hijack()
	if err != nil {
		log.Printf("[wsproxy] hijack fail: %v", err)
		return
	}
	defer client.Close()

	accept := wsAccept(key)
	rw.WriteString("HTTP/1.1 101 Switching Protocols\r\n")
	rw.WriteString("Upgrade: websocket\r\n")
	rw.WriteString("Connection: Upgrade\r\n")
	rw.WriteString("Sec-WebSocket-Accept: " + accept + "\r\n\r\n")
	if err := rw.Flush(); err != nil {
		return
	}

	target, err := net.DialTimeout("tcp", "127.0.0.1:"+port, 5*time.Second)
	if err != nil {
		log.Printf("[wsproxy] dial %s fail: %v", port, err)
		wsWriteClose(client, 1011, "target unreachable")
		return
	}
	defer target.Close()
	log.Printf("[wsproxy] OPEN %s from=%s", port, r.RemoteAddr)

	done := make(chan struct{}, 2)
	// WebSocket -> TCP
	go func() {
		defer func() { done <- struct{}{} }()
		for {
			op, payload, err := wsReadFrame(client)
			if err != nil {
				return
			}
			switch op {
			case 0x8: // close
				return
			case 0x9: // ping -> pong
				_ = wsWriteFrame(client, 0xA, payload)
			case 0xA: // pong
			default: // text/binary/continuation
				if len(payload) > 0 {
					if _, err := target.Write(payload); err != nil {
						return
					}
				}
			}
		}
	}()
	// TCP -> WebSocket
	go func() {
		defer func() { done <- struct{}{} }()
		buf := make([]byte, 4096)
		for {
			n, err := target.Read(buf)
			if n > 0 {
				if werr := wsWriteFrame(client, 0x2, buf[:n]); werr != nil {
					return
				}
			}
			if err != nil {
				return
			}
		}
	}()
	<-done
	log.Printf("[wsproxy] CLOSE %s", port)
}

func wsAccept(key string) string {
	h := sha1.New()
	io.WriteString(h, key+"258EAFA5-E914-47DA-95CA-C5AB0DC85B11")
	return base64.StdEncoding.EncodeToString(h.Sum(nil))
}

func wsReadFrame(r io.Reader) (byte, []byte, error) {
	var hdr [2]byte
	if _, err := io.ReadFull(r, hdr[:]); err != nil {
		return 0, nil, err
	}
	fin := hdr[0]&0x80 != 0
	op := hdr[0] & 0x0F
	masked := hdr[1]&0x80 != 0
	l := uint64(hdr[1] & 0x7F)
	switch l {
	case 126:
		var ext [2]byte
		if _, err := io.ReadFull(r, ext[:]); err != nil {
			return 0, nil, err
		}
		l = uint64(binary.BigEndian.Uint16(ext[:]))
	case 127:
		var ext [8]byte
		if _, err := io.ReadFull(r, ext[:]); err != nil {
			return 0, nil, err
		}
		l = binary.BigEndian.Uint64(ext[:])
	}
	if l > 16<<20 {
		return 0, nil, fmt.Errorf("frame too large: %d", l)
	}
	var mask [4]byte
	if masked {
		if _, err := io.ReadFull(r, mask[:]); err != nil {
			return 0, nil, err
		}
	}
	payload := make([]byte, l)
	if _, err := io.ReadFull(r, payload); err != nil {
		return 0, nil, err
	}
	if masked {
		for i := range payload {
			payload[i] ^= mask[i%4]
		}
	}
	if !fin {
		// 简化：拼接后续续帧
		next, more, err := wsReadFrame(r)
		if err != nil {
			return 0, nil, err
		}
		return next, append(payload, more...), nil
	}
	return op, payload, nil
}

func wsWriteFrame(w io.Writer, op byte, payload []byte) error {
	var b []byte
	b = append(b, 0x80|op)
	n := len(payload)
	switch {
	case n < 126:
		b = append(b, byte(n))
	case n < 65536:
		b = append(b, 126, 0, 0)
		binary.BigEndian.PutUint16(b[len(b)-2:], uint16(n))
	default:
		b = append(b, 127, 0, 0, 0, 0, 0, 0, 0, 0)
		binary.BigEndian.PutUint64(b[len(b)-8:], uint64(n))
	}
	b = append(b, payload...)
	_, err := w.Write(b)
	return err
}

func wsWriteClose(w io.Writer, code uint16, reason string) {
	var p []byte
	p = append(p, 0, 0)
	binary.BigEndian.PutUint16(p, code)
	p = append(p, []byte(reason)...)
	_ = wsWriteFrame(w, 0x8, p)
}
