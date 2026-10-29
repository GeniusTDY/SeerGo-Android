#!/usr/bin/env python3
"""Touch test on the pinggy page (already at server list)."""
import sys, time, base64
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget

p = [x for x in jget("/json/list") if "pinggy" in x["url"]][0]
s = Session(p["webSocketDebuggerUrl"])
s.ws.settimeout(200)
s.send("Emulation.setTouchEmulationEnabled", enabled=True, maxTouchPoints=1)

def shot(path):
    for i in range(4):
        try:
            r = s.send("Page.captureScreenshot", format="png")
            open(path, "wb").write(base64.b64decode(r["data"]))
            print("SHOT", path); return
        except Exception as e:
            print("retry", i, type(e).__name__); time.sleep(4)

def tap(x, y, hold=0.09):
    print("tap", x, y)
    s.send("Input.dispatchTouchEvent", type="touchStart", touchPoints=[{"x": x, "y": y}])
    time.sleep(hold)
    s.send("Input.dispatchTouchEvent", type="touchEnd", touchPoints=[])

shot("/opt/work/ts_0before.png")
tap(330, 164)   # 双击服务器行 -> 用两次
time.sleep(0.35)
tap(330, 164)
time.sleep(55)
shot("/opt/work/ts_1map.png")
tap(520, 420)
time.sleep(6)
shot("/opt/work/ts_2after.png")
