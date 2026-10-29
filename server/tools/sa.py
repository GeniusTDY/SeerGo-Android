#!/usr/bin/env python3
"""Stage A: fresh page -> title -> 开始 -> login screenshot."""
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
s.send("Page.navigate", url="http://127.0.0.1:41520/?clean=2")

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

def shot(f):
    for i in range(3):
        try:
            r = s.send("Page.captureScreenshot", format="png")
            open(f, "wb").write(base64.b64decode(r["data"]))
            print("SHOT", f, flush=True); return
        except Exception as e:
            print("re", type(e).__name__, flush=True); time.sleep(3)

time.sleep(75)
shot("/opt/work/x0_title.png")
click(480, 517, 12)
shot("/opt/work/x1_login.png")
