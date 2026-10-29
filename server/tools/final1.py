#!/usr/bin/env python3
"""最终验证 v3: 50003 新手教程 -> 翻到第9页 -> 点金装备"""
import sys, json, base64, time
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"]); s.ws.settimeout(90)
s.send("Page.enable"); s.send("Network.enable"); s.send("Runtime.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
bs = Session(jget("/json/version")["webSocketDebuggerUrl"])
bs.send("Target.activateTarget", targetId=page["id"])
s.send("Page.navigate", url="http://127.0.0.1:41520/?clean=%d" % int(time.time()))

def ev(e):
    return s.send("Runtime.evaluate", expression=e, returnByValue=True).get("result",{}).get("value")

def wait_player(limit=120):
    t0=time.time()
    while time.time()-t0 < limit:
        try:
            if ev("!!document.querySelector('ruffle-player')"):
                print("player", round(time.time()-t0), flush=True); return
        except Exception: pass
        time.sleep(2)

def rect():
    v = ev("JSON.stringify((function(){var p=document.querySelector('ruffle-player');var q=p.getBoundingClientRect();return [q.x,q.y,q.width,q.height]})())")
    return json.loads(v) if v else None

R = None
def click(sx, sy, wait=0.6, dbl=False):
    global R
    if R is None:
        R = rect()
    x = R[0]+sx/960.0*R[2]; y = R[1]+sy/560.0*R[3]
    for c in ([1,2] if dbl else [1]):
        s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x,y=y,button="none",buttons=0); time.sleep(.12)
        s.send("Input.dispatchMouseEvent", type="mousePressed", x=x,y=y,button="left",buttons=1,clickCount=c); time.sleep(.10)
        s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x,y=y,button="left",buttons=0,clickCount=c); time.sleep(.08)
    print("click", sx, sy, flush=True); time.sleep(wait)

def shot(name):
    try:
        z = s.send("Page.captureScreenshot", format="png")
        open("/opt/work/%s.png" % name, "wb").write(base64.b64decode(z["data"]))
        print("SHOT", name, flush=True)
    except Exception as e:
        print("shotfail", type(e).__name__, flush=True)

wait_player()
time.sleep(50); shot("f0_boot")
click(468, 509, 1.2, dbl=True); time.sleep(3); shot("f1_start")
click(519, 161, 0.8)
s.send("Input.insertText", text="test@test.com"); time.sleep(0.4)
click(519, 195, 0.8)
s.send("Input.insertText", text="123456"); time.sleep(0.4)
shot("f2_form")
click(452, 295, 12); shot("f3_server")
click(292, 193, 0.4); click(292, 193, 40); shot("f4_map")
for i in range(3):
    time.sleep(10); shot("f5_wait%d" % i)
print("advance pages", flush=True)
for i in range(9):
    click(713, 466, 1.2); shot("g%d" % i)
print("click item", flush=True)
click(707, 460, 2.5); shot("h1_item")
print("DONE", flush=True)
