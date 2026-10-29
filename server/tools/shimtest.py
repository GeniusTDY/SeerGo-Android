#!/usr/bin/env python3
"""Verify built-in touch shim in index.html."""
import sys, time, base64
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"])
s.ws.settimeout(200)
s.send("Page.enable"); s.send("Network.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
s.send("Emulation.setTouchEmulationEnabled", enabled=True, maxTouchPoints=1)
bs = Session(jget("/json/version")["webSocketDebuggerUrl"])
bs.send("Target.activateTarget", targetId=page["id"])
s.send("Page.navigate", url="http://127.0.0.1:41520/?shim=1")

def shot(p):
    r = s.send("Page.captureScreenshot", format="png")
    open(p, "wb").write(base64.b64decode(r["data"]))
    print("SHOT", p, flush=True)

def tap(x, y):
    s.send("Input.dispatchTouchEvent", type="touchStart", touchPoints=[{"x": x, "y": y}])
    time.sleep(0.1)
    s.send("Input.dispatchTouchEvent", type="touchEnd", touchPoints=[])
    print("tap", x, y, flush=True)

time.sleep(45)
shot("/opt/work/d1_title.png")
tap(490, 440)
time.sleep(10)
shot("/opt/work/d2_login.png")
tap(507, 136); s.send("Input.insertText", text="test@niao.local"); time.sleep(0.3)
tap(507, 166); s.send("Input.insertText", text="123456"); time.sleep(0.3)
shot("/opt/work/d3_filled.png")
tap(467, 256); time.sleep(14)
shot("/opt/work/d4_next.png")
