#!/usr/bin/env python3
"""Repro: decor click vs ground click on map 1 (mouse)."""
import sys, time, base64, json
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"])
s.ws.settimeout(240)
s.send("Page.enable"); s.send("Network.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
bs = Session(jget("/json/version")["webSocketDebuggerUrl"])
bs.send("Target.activateTarget", targetId=page["id"])
s.send("Page.navigate", url="http://127.0.0.1:41520/?clean=1")

def shot(f):
    for i in range(3):
        try:
            r = s.send("Page.captureScreenshot", format="png")
            open(f, "wb").write(base64.b64decode(r["data"]))
            print("SHOT", f, flush=True); return
        except Exception as e:
            print("re", type(e).__name__, flush=True); time.sleep(3)

def rect():
    r = s.send("Runtime.evaluate", expression="JSON.stringify(document.querySelector('ruffle-player').getBoundingClientRect())", returnByValue=True)
    return json.loads(r["result"]["value"])

def click(sx, sy, wait=1.0):
    rx = rect()
    x = rx["x"] + sx / 960.0 * rx["width"]
    y = rx["y"] + sy / 560.0 * rx["height"]
    s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x, y=y, button="none", buttons=0)
    time.sleep(0.08)
    s.send("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", buttons=1, clickCount=1)
    time.sleep(0.08)
    s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", buttons=0, clickCount=1)
    print("click", sx, sy, flush=True)
    time.sleep(wait)

time.sleep(40)
click(480, 517, 10); shot("/opt/work/r0_login.png")
click(519, 161, 0.4)
s.send("Input.insertText", text="test@niao.local"); time.sleep(0.3)
click(519, 195, 0.4)
s.send("Input.insertText", text="123456"); time.sleep(0.3)
click(452, 295, 16); shot("/opt/work/r1_server.png")
click(292, 193, 0.35)
click(292, 193, 70); shot("/opt/work/r2_map.png")
click(700, 300, 4); shot("/opt/work/r3_decor.png")
click(500, 420, 4); shot("/opt/work/r4_ground.png")
