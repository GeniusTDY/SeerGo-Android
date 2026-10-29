#!/usr/bin/env python3
"""诊断 v2: 重启 Chrome 后重跑，直连对话框 -> 点击 -> 读 trace"""
import sys, json, base64, time
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"])
s.ws.settimeout(120)
s.send("Page.enable"); s.send("Network.enable"); s.send("Runtime.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
bs = Session(jget("/json/version")["webSocketDebuggerUrl"])
bs.send("Target.activateTarget", targetId=page["id"])
s.send("Page.navigate", url="http://127.0.0.1:41520/?clean=%d" % int(time.time()))

LOG = open("/opt/work/dbg2_console.log", "w", encoding="utf-8")

def grab(tag=""):
    for m in s.drain(0.4):
        if m.get("method") == "Runtime.consoleAPICalled":
            args = m.get("params", {}).get("args", [])
            txt = " ".join(str(a.get("value", "")) for a in args)
            LOG.write(txt + "\n"); LOG.flush()
            if "[NPC]" in txt or "[NOVICE]" in txt:
                print("TRACE", txt, flush=True)

def ev(expr, timeout=10):
    return s.send("Runtime.evaluate", expression=expr, returnByValue=True).get("result",{}).get("value")

def wait_player(limit=150):
    t0 = time.time()
    while time.time() - t0 < limit:
        try:
            v = ev("!!document.querySelector('ruffle-player')")
        except Exception as e:
            v = None
        grab()
        if v:
            print("player ready after", round(time.time()-t0), flush=True)
            return True
        time.sleep(2)
    print("player NOT ready", flush=True)
    return False

def rect():
    v = ev("JSON.stringify((function(){var p=document.querySelector('ruffle-player');if(!p)return null;var q=p.getBoundingClientRect();return [q.x,q.y,q.width,q.height]})())")
    return json.loads(v) if v and v != "null" else None

def click(sx, sy, wait=0.5, dbl=False):
    R = rect()
    if not R:
        print("no player", flush=True); return
    x = R[0] + sx / 960.0 * R[2]; y = R[1] + sy / 560.0 * R[3]
    for n in ([1, 2] if dbl else [1]):
        s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x, y=y, button="none", buttons=0); time.sleep(.10)
        s.send("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", buttons=1, clickCount=n); time.sleep(.10)
        s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", buttons=0, clickCount=n); time.sleep(.06)
    print("click", sx, sy, flush=True)
    time.sleep(wait); grab()

def shot(name):
    try:
        z = s.send("Page.captureScreenshot", format="png")
        open("/opt/work/%s.png" % name, "wb").write(base64.b64decode(z["data"]))
        print("SHOT", name, flush=True)
    except Exception as e:
        print("shotfail", type(e).__name__, flush=True)

wait_player()
time.sleep(45); grab(); shot("v0_boot")
click(480, 517, 8); shot("v1_start")
click(519, 161, 0.8)
s.send("Input.insertText", text="test@niao.local"); time.sleep(0.5)
click(519, 195, 0.8)
s.send("Input.insertText", text="123456"); time.sleep(0.5)
click(452, 295, 16); shot("v2_login")
click(292, 193, 0.5); click(292, 193, 55); shot("v3_map")
for i in range(4):
    time.sleep(12); grab(); shot("v4_wait%d" % i)
print("=== now click ===", flush=True)
click(713, 466, 3.0); grab(); shot("v5_btn")
click(600, 380, 2.0); grab(); shot("v6_panel")
click(713, 466, 3.0, dbl=True); grab(); shot("v7_btn2")
LOG.close()
print("DONE", flush=True)
