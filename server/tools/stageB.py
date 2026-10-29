#!/usr/bin/env python3
"""Fresh page -> title -> click 开始 -> poll until login UI appears."""
import sys, time, base64, json, urllib.request
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget, new_page

TAG = sys.argv[1] if len(sys.argv) > 1 else "b"

# close stale game pages (keep at most 1)
pages = [p for p in jget("/json/list") if "41520" in p.get("url", "")]
bs = Session(jget("/json/version")["webSocketDebuggerUrl"])
for p in pages:
    try:
        bs.send("Target.closeTarget", targetId=p["id"])
    except Exception:
        pass
time.sleep(2)

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"])
s.ws.settimeout(240)
s.send("Page.enable"); s.send("Network.enable"); s.send("Runtime.enable"); s.send("Log.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
bs.send("Target.activateTarget", targetId=page["id"])
s.send("Page.navigate", url="http://127.0.0.1:41520/?v=sv" + str(int(time.time())))

def shot(name):
    for _ in range(3):
        try:
            r = s.send("Page.captureScreenshot", format="png")
            open("/opt/work/%s_%s.png" % (TAG, name), "wb").write(base64.b64decode(r["data"]))
            print("SHOT", name, flush=True)
            return
        except Exception as e:
            print("shot-retry", type(e).__name__, flush=True); time.sleep(2)

def rect():
    r = s.send("Runtime.evaluate", expression="JSON.stringify(document.querySelector('ruffle-player').getBoundingClientRect())", returnByValue=True)
    return json.loads(r["result"]["value"])

def click_screen(x, y):
    s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x, y=y, button="none", buttons=0)
    time.sleep(0.1)
    s.send("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", buttons=1, clickCount=1)
    time.sleep(0.1)
    s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", buttons=0, clickCount=1)
    time.sleep(0.3)

def click_stage(sx, sy):
    r = rect()
    click_screen(r["x"] + sx / 960.0 * r["width"], r["y"] + sy / 560.0 * r["height"])

time.sleep(45)
shot("00_boot")
click_stage(480, 517)   # 开始
time.sleep(3)
for i, t in enumerate([20, 20, 20, 20, 20], 1):
    time.sleep(t)
    shot("1%02d" % i)
print("done", flush=True)
