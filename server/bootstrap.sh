#!/usr/bin/env bash
# 沙箱重置后的一键恢复：铺文件 -> (必要时)编译 -> 启动四端口服务
# 用法：bash /workspace/server/bootstrap.sh
# 可选环境变量：SEER_ROOT(默认 /opt/seer)、NIEO_DATA(默认 /opt/NieoData)、NO_START=1 只铺文件不启动
#
# 说明：/opt/NieoData/resource 等 2.6G 资源不随仓库保存，服务端 auto-fetch 会在缺资源时
# 自动从官服回填；这里只铺网页入口、ruffle 运行时、配置和打过补丁的核心 SWF。
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SEER="${SEER_ROOT:-/opt/seer}"
NIEO="${NIEO_DATA:-/opt/NieoData}"

echo "[1/4] 服务端源码/存档 -> $SEER"
mkdir -p "$SEER"
rsync -a "$ROOT/backend/" "$SEER/"

echo "[2/4] 网页与资源覆盖层 -> $NIEO"
mkdir -p "$NIEO"
rsync -a "$ROOT/web/" "$NIEO/"

# 可选的资源缓存快照（/opt/NieoData/resource 不收在 web/ 里，体积大）
# 生成快照：rsync -a /opt/NieoData/resource/ /workspace/server/resource_cache/
if [ -d "$ROOT/resource_cache" ]; then
  echo "  资源缓存快照 -> $NIEO/resource"
  mkdir -p "$NIEO/resource"
  rsync -a "$ROOT/resource_cache/" "$NIEO/resource/"
fi

echo "[3/4] 检查可执行文件"
if [ ! -x "$SEER/output/nieoserver" ]; then
  echo "  未找到二进制，尝试 go build ..."
  ( cd "$SEER" && go build -o output/nieoserver ./cmd/nieoserver )
fi

if [ "${NO_START:-0}" = "1" ]; then
  echo "[4/4] NO_START=1，跳过启动"
  exit 0
fi

echo "[4/4] 启动服务"
if pgrep -f "output/nieoserver -root $SEER" >/dev/null 2>&1; then
  echo "  已在运行，跳过启动"
else
  nohup "$SEER/output/nieoserver" -root "$SEER" >> "$SEER/server.log" 2>&1 &
fi
sleep 3
curl -s -o /dev/null -m 3 "http://127.0.0.1:41520/" && echo "  RES 41520 OK" || echo "  RES 41520 FAIL"
curl -s -o /dev/null -m 3 "http://127.0.0.1:29180/" && echo "  GM  29180 OK" || echo "  GM  29180 FAIL"
curl -s -m 3 "http://127.0.0.1:41520/config/dll.xml" | grep -o 'dll/NieoCore[^"]*' || true
