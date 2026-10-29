#!/usr/bin/env python3
"""对当前 t8 页面截图/点击：shot NAME | click SX SY"""
import sys, json, base64, time
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget

pages = [x for x in jget("/json/list") if x["type"] == "page" and "q=" in x["url"]]
p = pages[0]
s = Session(p["webSocketDebuggerUrl"]); s.ws.settimeout(120)
s.send("Runtime.enable")
R = json.loads(s.send("Runtime.evaluate", expression="JSON.stringify((function(){var p=document.querySelector('ruffle-player');var q=p.getBoundingClientRect();return [q.x,q.y,q.width,q.height]})())", returnByValue=True)["result"]["value"])

def click(sx, sy, wait=1.0):
    x = R[0] + sx / 960.0 * R[2]; y = R[1] + sy / 560.0 * R[3]
    s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x, y=y, button="none", buttons=0); time.sleep(.12)
    s.send("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", buttons=1, clickCount=1); time.sleep(.1)
    s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", buttons=0, clickCount=1); time.sleep(.08)
    print(time.strftime("%H:%M:%S"), "click", sx, sy, flush=True); time.sleep(wait)

def shot(name):
    z = s.send("Page.captureScreenshot", format="png")
    open("/opt/work/%s.png" % name, "wb").write(base64.b64decode(z["data"]))
    print(time.strftime("%H:%M:%S"), "SHOT", name, flush=True)

for a in sys.argv[1:]:
    if a.startswith("shot:"):
        shot(a[5:])
    elif a.startswith("click:"):
        _, sx, sy = a.split(":")
        click(float(sx), float(sy))
