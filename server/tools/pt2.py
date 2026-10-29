#!/usr/bin/env python3
"""Single-session portrait flow: title -> tap start -> screenshot."""
import sys, time, base64
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

W, H = 430, 900
K = min(W / 560.0, H / 960.0)

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"])
s.ws.settimeout(200)
s.send("Page.enable")
s.send("Network.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
s.send("Emulation.setDeviceMetricsOverride", width=W, height=H, deviceScaleFactor=2, mobile=True)
s.send("Emulation.setTouchEmulationEnabled", enabled=True, maxTouchPoints=1)
ver = jget("/json/version")
bs = Session(ver["webSocketDebuggerUrl"])
bs.send("Target.activateTarget", targetId=page["id"])
s.send("Page.navigate", url="http://127.0.0.1:41520/?pv=2")

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

def tap(sx, sy, hold=0.08):
    x = W / 2.0 - K * (sy - 280.0)
    y = H / 2.0 + K * (sx - 480.0)
    print("tap", sx, sy, "->", round(x, 1), round(y, 1))
    s.send("Input.dispatchTouchEvent", type="touchStart", touchPoints=[{"x": x, "y": y}])
    time.sleep(hold)
    s.send("Input.dispatchTouchEvent", type="touchEnd", touchPoints=[])

time.sleep(40)
shot("/opt/work/pt_4title.png")
tap(480, 517)
time.sleep(9)
shot("/opt/work/pt_5login.png")
print("INNER", s.send("Runtime.evaluate", expression="JSON.stringify([innerWidth,innerHeight,document.getElementById('stage').style.transform])", returnByValue=True).get("result", {}).get("value"))
