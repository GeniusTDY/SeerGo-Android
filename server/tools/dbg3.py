#!/usr/bin/env python3
"""续跑: 登录 -> 进服 -> 截图 -> 点对话框/装备 -> 收 trace"""
import sys, json, base64, time
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget

p = [t for t in jget("/json/list") if "41520" in t.get("url","") and t["type"]=="page"][0]
s = Session(p["webSocketDebuggerUrl"]); s.ws.settimeout(120)
s.send("Page.enable"); s.send("Runtime.enable")
LOG = open("/opt/work/dbg3_console.log", "w", encoding="utf-8")

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

R = json.loads(ev("JSON.stringify((function(){var p=document.querySelector('ruffle-player');var q=p.getBoundingClientRect();return [q.x,q.y,q.width,q.height]})())"))
def click(sx, sy, wait=0.6, n=1):
    x = R[0]+sx/960.0*R[2]; y = R[1]+sy/560.0*R[3]
    for c in [n] if n==1 else [1,2]:
        s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x,y=y,button="none",buttons=0); time.sleep(.15)
        s.send("Input.dispatchMouseEvent", type="mousePressed", x=x,y=y,button="left",buttons=1,clickCount=c); time.sleep(.12)
        s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x,y=y,button="left",buttons=0,clickCount=c); time.sleep(.1)
    print("click", sx, sy, flush=True); time.sleep(wait); grab()

def shot(name):
    z = s.send("Page.captureScreenshot", format="png")
    open("/opt/work/%s.png" % name, "wb").write(base64.b64decode(z["data"]))
    print("SHOT", name, flush=True)

click(519, 161, 0.8)
s.send("Input.insertText", text="test@niao.local"); time.sleep(0.4)
click(519, 195, 0.8)
s.send("Input.insertText", text="123456"); time.sleep(0.4)
shot("w0_form")
click(452, 295, 14); shot("w1_server")
click(292, 193, 0.4); click(292, 193, 50); shot("w2_map")
for i in range(5):
    time.sleep(10); grab(); shot("w3_wait%d" % i)
print("=== click dialog ===", flush=True)
click(713, 466, 2.5); grab(); shot("w4_btn")
click(600, 380, 1.5); grab(); shot("w5_panel")
click(713, 466, 2.5); grab(); shot("w6_btn2")
LOG.close(); print("DONE", flush=True)
