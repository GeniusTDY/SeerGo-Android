#!/usr/bin/env python3
"""按配置表推导资源清单，通过本地 res 服务并发预热到 /opt/NieoData/resource。

用法：python3 warm_assets.py [--scope maps,pets,skills,misc] [--workers 8] [--limit N]
默认对 127.0.0.1:41520；拉不到的路径会被服务端负缓存 10 分钟，不会反复拖慢后续请求。
"""
import argparse, json, re, sys, time, urllib.request
from concurrent.futures import ThreadPoolExecutor

BASE = "http://127.0.0.1:41520"
TABLES = "/opt/seer/tables/xml"
MAP_CFG = "/opt/seer/data/map_wild_config.json"


def ids():
    out = {}
    maps = json.load(open(MAP_CFG))["maps"].keys()
    out["maps"] = ["/resource/map/%s.swf" % m for m in maps]
    pets = sorted({int(x) for x in re.findall(r'<Monster\b[^>]*\bID="(\d+)"', open(TABLES + "/pets.xml", errors="ignore").read())})
    out["pets"] = ["/resource/pet/swf/%d.swf" % p for p in pets] + \
                  ["/resource/fightResource/pet/swf/%03d.swf" % p for p in pets]
    skills = sorted({int(x) for x in re.findall(r'<Move\b[^>]*\bID="(\d+)"', open(TABLES + "/SkillXMLInfo.xml", errors="ignore").read())})
    out["skills"] = ["/resource/fightResource/skill/swf/%d.swf" % s for s in skills]
    out["misc"] = ["/resource/ui.swf", "/resource/aimat/aimatUI.swf",
                   "/resource/module/PetFightDLL.swf", "/dll/Assets.swf"]
    return out


def fetch(url, timeout=40):
    try:
        with urllib.request.urlopen(BASE + url, timeout=timeout) as r:
            return url, r.status, len(r.read())
    except Exception as e:
        return url, 0, 0


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--scope", default="maps,pets,skills,misc")
    ap.add_argument("--workers", type=int, default=8)
    ap.add_argument("--limit", type=int, default=0)
    a = ap.parse_args()

    allids = ids()
    urls = []
    for sc in a.scope.split(","):
        urls += allids.get(sc.strip(), [])
    if a.limit:
        urls = urls[: a.limit]
    print("total=%d scope=%s workers=%d" % (len(urls), a.scope, a.workers), flush=True)

    t0, ok, bytes_, fails = time.time(), 0, 0, []
    with ThreadPoolExecutor(max_workers=a.workers) as ex:
        for i, (u, code, n) in enumerate(ex.map(fetch, urls), 1):
            if code == 200 and n > 0:
                ok += 1
                bytes_ += n
            else:
                fails.append(u)
            if i % 500 == 0:
                print("  %d/%d ok=%d bytes=%.1fMB %.0fs" % (i, len(urls), ok, bytes_ / 1048576, time.time() - t0), flush=True)

    dt = time.time() - t0
    print("done ok=%d fail=%d bytes=%.1fMB time=%.0fs" % (ok, len(fails), bytes_ / 1048576, dt))
    if fails:
        open("/workspace/server/warm_fails.txt", "w").write("\n".join(fails))
        print("fail list -> /workspace/server/warm_fails.txt")


if __name__ == "__main__":
    main()
