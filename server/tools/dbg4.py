#!/usr/bin/env python3
"""诊断 v3: test@test.com (uid 50003, 新手教程) -> 直连对话框 -> 8 次继续 -> 点装备"""
import sys, json, base64, time
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"]); s.ws.settimeout(150)
s.send("Page.enable"); s.send("Network.enable"); s.send("Runtime.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
bs = Session(jget("/json/version")["webSocketDebuggerUrl"])
bs.send("Target.activateTarget", targetId=page["id"])
s.send("Page.navigate", url="http://127.0.0.1:41520/?clean=%d" % int(time.time()))
LOG = open("/opt/work/dbg4_console.log", "w", encoding="utf-8")

def grab():
    for m in s.drain(0.4):
        if m.get("method") == "Runtime.consoleAPICalled":
            args = m.get("params", {}).get("args", [])
            txt = " ".join(str(a.get("value", "")) for a in args)
            LOG.write(txt + "\n"); LOG.flush()
            if "[NPC]" in txt or "[NOVICE]" in txt:
                print("TRACE", txt, flush=True)

def ev(e):
    return s.send("Runtime.evaluate", expression=e, returnByValue=True).get("result",{}).get("value")

def wait_player(limit=150):
    t0=time.time()
    while time.time()-t0 < limit:
        try:
            if ev("!!document.querySelector('ruffle-player')"):
                print("player after", round(time.time()-t0), flush=True); return
        except Exception: pass
        grab(); time.sleep(2)
    print("player missing", flush=True)

def rect():
    v = ev("JSON.stringify((function(){var p=document.querySelector('ruffle-player');var q=p.getBoundingClientRect();return [q.x,q.y,q.width,q.height]})())")
    return json.loads(v)

R = None
def click(sx, sy, wait=0.6, dbl=False):
    global R
    if R is None: R = rect()
    x = R[0]+sx/960.0*R[2]; y = R[1]+sy/560.0*R[3]
    for c in ([1,2] if dbl else [1]):
        s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x,y=y,button="none",buttons=0); time.sleep(.15)
        s.send("Input.dispatchMouseEvent", type="mousePressed", x=x,y=y,button="left",buttons=1,clickCount=c); time.sleep(.12)
        s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x,y=y,button="left",buttons=0,clickCount=c); time.sleep(.1)
    print("click", sx, sy, flush=True); time.sleep(wait); grab()

def shot(name):
    z = s.send("Page.captureScreenshot", format="png")
    open("/opt/work/%s.png" % name, "wb").write(base64.b64decode(z["data"]))
    print("SHOT", name, flush=True)

wait_player()
time.sleep(50); grab(); shot("x0_boot")
click(468, 509, 1.0, dbl=True); time.sleep(4); shot("x1_start")
click(519, 161, 0.8)
s.send("Input.insertText", text="test@test.com"); time.sleep(0.5)
click(519, 195, 0.8)
s.send("Input.insertText", text="123456"); time.sleep(0.5)
shot("x2_form")
click(452, 295, 15); shot("x3_server")
click(292, 193, 0.4); click(292, 193, 45); shot("x4_map")
for i in range(4):
    time.sleep(10); grab(); shot("x5_wait%d" % i)
tested = 0
for i in range(9):
    click(713, 466, 1.5); grab()
    shot("y%d" % i)
print("done pages", flush=True)
click(713, 466, 2.0); grab(); shot("z1_item")
LOG.close(); print("DONE", flush=True)
