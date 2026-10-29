#!/usr/bin/env python3
"""Stage A: fresh page -> title -> login -> server list (touch input)."""
import sys, time, base64, json
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
s.send("Page.navigate", url="http://127.0.0.1:41520/?t=1")
print("PAGE", page["id"], flush=True)

def shot(p):
    for i in range(3):
        try:
            r = s.send("Page.captureScreenshot", format="png")
            open(p, "wb").write(base64.b64decode(r["data"])); print("SHOT", p, flush=True); return
        except Exception as e:
            print("re", type(e).__name__, flush=True); time.sleep(3)

def rect():
    r = s.send("Runtime.evaluate", expression="JSON.stringify(document.querySelector('ruffle-player').getBoundingClientRect())", returnByValue=True)
    return json.loads(r["result"]["value"])

def tap_stage(sx, sy, hold=0.1):
    rc = rect()
    x = rc["x"] + sx / 960.0 * rc["width"]
    y = rc["y"] + sy / 560.0 * rc["height"]
    s.send("Input.dispatchTouchEvent", type="touchStart", touchPoints=[{"x": x, "y": y}])
    time.sleep(hold)
    s.send("Input.dispatchTouchEvent", type="touchEnd", touchPoints=[])
    print("tap", sx, sy, "->", round(x), round(y), flush=True)
    time.sleep(0.4)

time.sleep(45); shot("/opt/work/a1_title.png")
tap_stage(480, 517); time.sleep(10); shot("/opt/work/a2_login.png")
tap_stage(500, 160); s.send("Input.insertText", text="test@niao.local"); time.sleep(0.4)
tap_stage(500, 195); s.send("Input.insertText", text="123456"); time.sleep(0.4)
shot("/opt/work/a3_filled.png")
tap_stage(453, 300); time.sleep(14); shot("/opt/work/a4_next.png")
rectv = rect(); print("RECT", rectv, flush=True)
print("STATE", s.send("Runtime.evaluate", expression="document.getElementById('err').textContent", returnByValue=True).get("result", {}).get("value"), flush=True)
