#!/usr/bin/env python3
"""诊断: 直连对话框 -> 点击 -> 读取 [NPC]/[NOVICE] trace"""
import sys, json, base64, time, re
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"])
s.ws.settimeout(300)
s.send("Page.enable"); s.send("Network.enable"); s.send("Runtime.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
bs = Session(jget("/json/version")["webSocketDebuggerUrl"])
bs.send("Target.activateTarget", targetId=page["id"])
s.send("Page.navigate", url="http://127.0.0.1:41520/?clean=%d" % int(time.time()))

LOG = open("/opt/work/dbg1_console.log", "w", encoding="utf-8")

def grab(tag=""):
    for m in s.drain(0.4):
        if m.get("method") == "Runtime.consoleAPICalled":
            args = m.get("params", {}).get("args", [])
            txt = " ".join(str(a.get("value", "")) for a in args)
            LOG.write(txt + "\n"); LOG.flush()
            if "[NPC]" in txt or "[NOVICE]" in txt:
                print("TRACE", txt, flush=True)
        elif m.get("method") == "Log.entryAdded":
            txt = json.dumps(m.get("params", {}), ensure_ascii=False)
            LOG.write("LOG " + txt + "\n"); LOG.flush()

def rect():
    r = s.send("Runtime.evaluate", expression="JSON.stringify((function(){var p=document.querySelector('ruffle-player');if(!p)return null;var q=p.getBoundingClientRect();return [q.x,q.y,q.width,q.height]})())", returnByValue=True)
    v = r.get("result", {}).get("value")
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
        print("shotfail", e, flush=True)

time.sleep(50); grab(); shot("d0_boot")
click(480, 517, 6); shot("d1_start")
click(519, 161, 0.6)
s.send("Input.insertText", text="test@niao.local"); time.sleep(0.4)
click(519, 195, 0.6)
s.send("Input.insertText", text="123456"); time.sleep(0.4)
click(452, 295, 14); shot("d2_login")
click(292, 193, 0.4); click(292, 193, 60); shot("d3_map")
# 等地图与教程
for i in range(6):
    time.sleep(10); grab(); shot("d4_wait%d" % i)
# 点击 ▼ 与面板区域
click(713, 466, 2.0); grab(); shot("e1_btn")
click(600, 380, 2.0); grab(); shot("e2_panel")
click(713, 466, 3.0, dbl=True); grab(); shot("e3_btn2")
LOG.close()
print("DONE", flush=True)
