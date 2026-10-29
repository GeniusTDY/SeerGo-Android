#!/usr/bin/env python3
"""Full touch flow: title -> login -> server -> map -> tap ground."""
import sys, time, base64
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"])
s.ws.settimeout(240)
s.send("Page.enable"); s.send("Network.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
s.send("Emulation.setTouchEmulationEnabled", enabled=True, maxTouchPoints=1)
bs = Session(jget("/json/version")["webSocketDebuggerUrl"])
bs.send("Target.activateTarget", targetId=page["id"])
s.send("Page.navigate", url="http://127.0.0.1:41520/?shim=2")

def shot(p):
    for i in range(3):
        try:
            r = s.send("Page.captureScreenshot", format="png")
            open(p, "wb").write(base64.b64decode(r["data"]))
            print("SHOT", p, flush=True)
            return
        except Exception as e:
            print("re", type(e).__name__, flush=True); time.sleep(3)

def tap(x, y, wait=0.0):
    s.send("Input.dispatchTouchEvent", type="touchStart", touchPoints=[{"x": x, "y": y}])
    time.sleep(0.1)
    s.send("Input.dispatchTouchEvent", type="touchEnd", touchPoints=[])
    if wait: time.sleep(wait)

time.sleep(45); shot("/opt/work/f1_title.png")
tap(490, 440, 10); shot("/opt/work/f2_login.png")
tap(507, 136); s.send("Input.insertText", text="test@niao.local"); time.sleep(0.3)
tap(507, 166); s.send("Input.insertText", text="123456"); time.sleep(0.3)
shot("/opt/work/f3_filled.png")
tap(467, 256, 15); shot("/opt/work/f4_after_login.png")
