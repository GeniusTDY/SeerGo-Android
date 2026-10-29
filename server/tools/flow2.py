#!/usr/bin/env python3
"""Continue from login page: fill, login, server, map, tap ground."""
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
    print("tap", x, y, flush=True)

tap(507, 136)
s.send("Input.insertText", text="test@niao.local"); time.sleep(0.4)
tap(507, 166)
s.send("Input.insertText", text="123456"); time.sleep(0.4)
shot("/opt/work/h1_filled.png")
tap(467, 256, 16)
shot("/opt/work/h2_after_login.png")
