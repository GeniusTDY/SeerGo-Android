#!/usr/bin/env python3
"""Click at stage-relative coordinate (960x560) inside the Ruffle page."""
import json, sys, time, base64
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget

SX = float(sys.argv[1]); SY = float(sys.argv[2])
WAIT = float(sys.argv[3]) if len(sys.argv) > 3 else 6
SHOT = sys.argv[4] if len(sys.argv) > 4 else "/opt/tools/click.png"

pages = [p for p in jget("/json/list") if "41520" in p.get("url", "")]
page = pages[0]
s = Session(page["webSocketDebuggerUrl"])
s.send("Runtime.enable")
res = s.send("Runtime.evaluate", expression="""
(function(){
  var c = document.querySelector('ruffle-player') || document.querySelector('canvas');
  var r = c.getBoundingClientRect();
  return JSON.stringify({x:r.x,y:r.y,w:r.width,h:r.height});
})()
""", returnByValue=True)
rect = json.loads(res["result"]["value"])
x = rect["x"] + SX / 960.0 * rect["w"]
y = rect["y"] + SY / 560.0 * rect["h"]
print("rect", rect, "->", x, y)

s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x, y=y, button="none", buttons=0)
time.sleep(0.15)
s.send("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", buttons=1, clickCount=1)
time.sleep(0.06)
s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", buttons=0, clickCount=1)
if len(sys.argv) > 5 and sys.argv[5] == "dbl":
    time.sleep(0.06)
    s.send("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", buttons=1, clickCount=2)
    time.sleep(0.06)
    s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", buttons=0, clickCount=2)
time.sleep(WAIT)
shot = s.send("Page.captureScreenshot", format="png")
open(SHOT, "wb").write(base64.b64decode(shot["data"]))
print("SHOT", SHOT)
