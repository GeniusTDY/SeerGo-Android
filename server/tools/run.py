#!/usr/bin/env python3
"""Navigate, activate, wait, screenshot; optionally click stage coords. Keeps one session alive."""
import sys, time, base64, json
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

URL = sys.argv[1]
ACTIONS = json.loads(sys.argv[2]) if len(sys.argv) > 2 else []
OUT = sys.argv[3] if len(sys.argv) > 3 else "/opt/work/run.png"

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"])
s.ws.settimeout(200)
s.send("Runtime.enable"); s.send("Network.enable"); s.send("Network.setCacheDisabled", cacheDisabled=True)
s.send("Page.enable"); s.send("Page.navigate", url=URL)

ver = jget("/json/version")
bs = Session(ver["webSocketDebuggerUrl"])
bs.send("Target.activateTarget", targetId=page["id"])

for act in ACTIONS:
    kind = act.get("do")
    if kind == "wait":
        time.sleep(act.get("s", 10))
    elif kind == "click":
        r = s.send("Runtime.evaluate", expression="(function(){var c=document.querySelector('ruffle-player');var r=c.getBoundingClientRect();return JSON.stringify({x:r.x,y:r.y,w:r.width,h:r.height});})()", returnByValue=True)
        rect = json.loads(r["result"]["value"])
        sx, sy = act["sx"], act["sy"]
        x = rect["x"] + sx / 960.0 * rect["w"]
        y = rect["y"] + sy / 560.0 * rect["h"]
        print("click", sx, sy, "->", x, y)
        s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x, y=y, button="none", buttons=0)
        time.sleep(0.15)
        s.send("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", buttons=1, clickCount=act.get("cc", 1))
        time.sleep(0.08)
        s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", buttons=0, clickCount=act.get("cc", 1))
    elif kind == "shot":
        path = act.get("file", OUT)
        for i in range(3):
            try:
                r = s.send("Page.captureScreenshot", format="png")
                open(path, "wb").write(base64.b64decode(r["data"]))
                print("SHOT", path)
                break
            except Exception as e:
                print("shot retry", i, type(e).__name__)
                time.sleep(5)
