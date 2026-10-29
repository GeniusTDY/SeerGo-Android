#!/usr/bin/env python3
"""Portrait-aware click: map stage coords (960x560) to screen accounting for rotate(90)+scale."""
import sys, time, base64, json
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget

SX = float(sys.argv[1]); SY = float(sys.argv[2])
WAIT = float(sys.argv[3]) if len(sys.argv) > 3 else 5
SHOT = sys.argv[4] if len(sys.argv) > 4 else "/opt/tools/pclick.png"
W = int(sys.argv[5]) if len(sys.argv) > 5 else 430
H = int(sys.argv[6]) if len(sys.argv) > 6 else 900

pages = [p for p in jget("/json/list") if "41520" in p.get("url", "")]
page = pages[0]
s = Session(page["webSocketDebuggerUrl"])
s.send("Emulation.setDeviceMetricsOverride", width=W, height=H, deviceScaleFactor=2, mobile=True)
time.sleep(1.2)
k = min(W / 560.0, H / 960.0)
if W < H:
    x = W / 2.0 - k * (SY - 280.0)
    y = H / 2.0 + k * (SX - 480.0)
else:
    k = min(W / 960.0, H / 560.0)
    x = W / 2.0 + k * (SX - 480.0)
    y = H / 2.0 + k * (SY - 280.0)
print("k", k, "screen", x, y)

s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x, y=y, button="none", buttons=0)
time.sleep(0.15)
s.send("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", buttons=1, clickCount=1)
time.sleep(0.06)
s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", buttons=0, clickCount=1)
time.sleep(WAIT)
shot = s.send("Page.captureScreenshot", format="png")
open(SHOT, "wb").write(base64.b64decode(shot["data"]))
print("SHOT", SHOT)
