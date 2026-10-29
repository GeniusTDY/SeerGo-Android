#!/usr/bin/env python3
import sys, time, base64, json
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget

U = "https://zdzgf-180-184-77-101.run.pinggy-free.link/"
pages = [p for p in jget("/json/list") if "pinggy" in p.get("url", "")]
if not pages:
    print("no page"); sys.exit(1)
page = pages[0]
s = Session(page["webSocketDebuggerUrl"])
s.ws.settimeout(200)

def shot(path):
    for i in range(3):
        try:
            r = s.send("Page.captureScreenshot", format="png")
            open(path, "wb").write(base64.b64decode(r["data"]))
            print("SHOT", path); return
        except Exception as e:
            print("retry", i, type(e).__name__); time.sleep(4)

def click(x, y):
    s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x, y=y, button="none", buttons=0)
    time.sleep(0.1)
    s.send("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", buttons=1, clickCount=1)
    time.sleep(0.06)
    s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", buttons=0, clickCount=1)

# 1. 过拦截页
click(430, 320)
time.sleep(6)
shot("/opt/work/pg2_a.png")

# 2. 等标题
time.sleep(20)
shot("/opt/work/pg2_b.png")

# 3. 点开始
r = s.send("Runtime.evaluate", expression="(function(){var c=document.querySelector('ruffle-player');if(!c)return 'none';var r=c.getBoundingClientRect();return JSON.stringify({x:r.x,y:r.y,w:r.width,h:r.height});})()", returnByValue=True)
v = r.get("result", {}).get("value")
print("rect", v)
if v and v != "none":
    rc = json.loads(v)
    x = rc["x"] + 480 / 960.0 * rc["w"]
    y = rc["y"] + 517 / 560.0 * rc["h"]
    click(x, y)
    time.sleep(15)
shot("/opt/work/pg2_c.png")
