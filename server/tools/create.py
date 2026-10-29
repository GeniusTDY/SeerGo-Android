#!/usr/bin/env python3
"""Create character: click nickname field, type, click confirm."""
import json, sys, time, base64
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget

NAME = sys.argv[1] if len(sys.argv) > 1 else "Nieo01"
WAIT = float(sys.argv[2]) if len(sys.argv) > 2 else 20

pages = [p for p in jget("/json/list") if "41520" in p.get("url", "")]
page = pages[0]
s = Session(page["webSocketDebuggerUrl"])
s.send("Runtime.enable")
rect = json.loads(s.send("Runtime.evaluate", expression="""
(function(){var c=document.querySelector('ruffle-player')||document.querySelector('canvas');
var r=c.getBoundingClientRect();return JSON.stringify({x:r.x,y:r.y,w:r.width,h:r.height});})()
""", returnByValue=True)["result"]["value"])

def click(sx, sy):
    x = rect["x"] + sx / 960.0 * rect["w"]
    y = rect["y"] + sy / 560.0 * rect["h"]
    s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x, y=y, button="none", buttons=0)
    time.sleep(0.1)
    s.send("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", buttons=1, clickCount=1)
    time.sleep(0.1)
    s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", buttons=0, clickCount=1)
    time.sleep(0.4)

def type_text(t):
    for ch in t:
        s.send("Input.dispatchKeyEvent", type="keyDown", text=ch, unmodifiedText=ch, key=ch)
        s.send("Input.dispatchKeyEvent", type="keyUp", key=ch)
        time.sleep(0.08)

def key(k, code, vk):
    s.send("Input.dispatchKeyEvent", type="rawKeyDown", key=k, code=code, windowsVirtualKeyCode=vk, nativeVirtualKeyCode=vk)
    time.sleep(0.05)
    s.send("Input.dispatchKeyEvent", type="keyUp", key=k, code=code, windowsVirtualKeyCode=vk, nativeVirtualKeyCode=vk)
    time.sleep(0.2)

click(741, 166)
type_text(NAME)
time.sleep(0.4)
shot = s.send("Page.captureScreenshot", format="png")
open("/opt/tools/shot_name.png", "wb").write(base64.b64decode(shot["data"]))

click(674, 450)
time.sleep(WAIT)
shot = s.send("Page.captureScreenshot", format="png")
open("/opt/tools/shot_create.png", "wb").write(base64.b64decode(shot["data"]))
print("done")
