#!/usr/bin/env python3
"""Click at stage coords on the current game page. usage: ck.py x y [double] [shotname]"""
import sys, json, base64, time
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget

sx, sy = float(sys.argv[1]), float(sys.argv[2])
dbl = len(sys.argv) > 3 and sys.argv[3] == "d"
shotn = sys.argv[4] if len(sys.argv) > 4 else None

p = [t for t in jget("/json/list") if "41520" in t.get("url", "") and t["type"] == "page"][0]
s = Session(p["webSocketDebuggerUrl"]); s.send("Page.enable")
r = json.loads(s.send("Runtime.evaluate", expression="JSON.stringify(document.querySelector('ruffle-player').getBoundingClientRect())", returnByValue=True)["result"]["value"])

def clk(count):
    x = r["x"] + sx / 960.0 * r["width"]; y = r["y"] + sy / 560.0 * r["height"]
    s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x, y=y, button="none", buttons=0); time.sleep(.15)
    s.send("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", buttons=1, clickCount=count); time.sleep(.12)
    s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", buttons=0, clickCount=count)
    print("click", sx, sy, "n=", count, "->", round(x), round(y), flush=True)

if dbl:
    clk(1); time.sleep(.12); clk(2)
else:
    clk(1)
time.sleep(1.5)
if shotn:
    time.sleep(1)
    z = s.send("Page.captureScreenshot", format="png")
    open("/opt/work/%s.png" % shotn, "wb").write(base64.b64decode(z["data"]))
    print("SHOT", shotn, flush=True)
