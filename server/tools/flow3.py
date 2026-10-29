#!/usr/bin/env python3
"""Enter map by touch, then tap ground and compare."""
import sys, time, base64
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget

p = [x for x in jget("/json/list") if "shim=2" in x["url"]][0]
s = Session(p["webSocketDebuggerUrl"])
s.ws.settimeout(240)
s.send("Emulation.setTouchEmulationEnabled", enabled=True, maxTouchPoints=1)

def shot(f):
    for i in range(3):
        try:
            r = s.send("Page.captureScreenshot", format="png")
            open(f, "wb").write(base64.b64decode(r["data"]))
            print("SHOT", f, flush=True); return
        except Exception as e:
            print("re", type(e).__name__, flush=True); time.sleep(3)

def tap(x, y, wait=0.0):
    s.send("Input.dispatchTouchEvent", type="touchStart", touchPoints=[{"x": x, "y": y}])
    time.sleep(0.12)
    s.send("Input.dispatchTouchEvent", type="touchEnd", touchPoints=[])
    if wait: time.sleep(wait)

tap(330, 164)
time.sleep(0.35)
tap(330, 164)
time.sleep(75)
shot("/opt/work/i1_map.png")
tap(520, 420, 6)
shot("/opt/work/i2_walk1.png")
tap(420, 300, 6)
shot("/opt/work/i3_walk2.png")
