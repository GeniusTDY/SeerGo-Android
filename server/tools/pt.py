#!/usr/bin/env python3
"""Portrait touch tester: map stage coords -> screen under rotate(90)+scale, dispatch touch."""
import sys, time, base64, json
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

URL = "http://127.0.0.1:41520/"
W, H = 430, 900
K = min(W / 560.0, H / 960.0)

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"])
s.ws.settimeout(200)
s.send("Page.enable")
s.send("Emulation.setDeviceMetricsOverride", width=W, height=H, deviceScaleFactor=2, mobile=True)
s.send("Emulation.setTouchEmulationEnabled", enabled=True, maxTouchPoints=1)
s.send("Page.navigate", url=URL)
ver = jget("/json/version")
bs = Session(ver["webSocketDebuggerUrl"])
bs.send("Target.activateTarget", targetId=page["id"])

def shot(path):
    for i in range(4):
        try:
            r = s.send("Page.captureScreenshot", format="png")
            open(path, "wb").write(base64.b64decode(r["data"]))
            print("SHOT", path)
            return
        except Exception as e:
            print("retry", i, type(e).__name__)
            time.sleep(4)

def tap(sx, sy):
    if W < H:
        x = W / 2.0 - K * (sy - 280.0)
        y = H / 2.0 + K * (sx - 480.0)
    else:
        x = W / 2.0 + K * (sx - 480.0)
        y = H / 2.0 + K * (sy - 280.0)
    print("tap stage", sx, sy, "-> screen", round(x, 1), round(y, 1))
    s.send("Input.dispatchTouchEvent", type="touchStart", touchPoints=[{"x": x, "y": y}])
    time.sleep(0.08)
    s.send("Input.dispatchTouchEvent", type="touchEnd", touchPoints=[])

time.sleep(30)
shot("/opt/work/pt_1title.png")
tap(480, 517)
time.sleep(8)
shot("/opt/work/pt_2after.png")
