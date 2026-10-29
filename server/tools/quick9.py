#!/usr/bin/env python3
"""快速验证：测试版核心进图即弹对话框 -> 8页 -> 直接点金装备（对话框仍打开）"""
import sys, json, base64, time
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"]); s.ws.settimeout(120)
s.send("Page.enable"); s.send("Network.enable"); s.send("Runtime.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
bs = Session(jget("/json/version")["webSocketDebuggerUrl"])
bs.send("Target.activateTarget", targetId=page["id"])
s.send("Page.navigate", url="http://127.0.0.1:41520/?q=%d" % int(time.time()))

def ev(e):
    return s.send("Runtime.evaluate", expression=e, returnByValue=True).get("result", {}).get("value")

R = None
def rect():
    v = ev("JSON.stringify((function(){var p=document.querySelector('ruffle-player');var q=p.getBoundingClientRect();return [q.x,q.y,q.width,q.height]})())")
    return json.loads(v) if v else None

def click(sx, sy, wait=0.8, dbl=False):
    global R
    if R is None:
        R = rect()
    x = R[0] + sx / 960.0 * R[2]
    y = R[1] + sy / 560.0 * R[3]
    for c in ([1, 2] if dbl else [1]):
        s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x, y=y, button="none", buttons=0); time.sleep(.12)
        s.send("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", buttons=1, clickCount=c); time.sleep(.10)
        s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", buttons=0, clickCount=c); time.sleep(.08)
    print(time.strftime("%H:%M:%S"), "click", sx, sy, flush=True); time.sleep(wait)

def shot(name):
    z = s.send("Page.captureScreenshot", format="png")
    open("/opt/work/%s.png" % name, "wb").write(base64.b64decode(z["data"]))
    print(time.strftime("%H:%M:%S"), "SHOT", name, flush=True)

t0 = time.time()
while time.time() - t0 < 120:
    try:
        if ev("!!document.querySelector('ruffle-player')"):
            break
    except Exception:
        pass
    time.sleep(2)
print(time.strftime("%H:%M:%S"), "player", flush=True)
time.sleep(50); shot("q0_boot")
click(468, 509, 1.2, dbl=True); time.sleep(3); shot("q1_start")
click(519, 161, 0.8)
s.send("Input.insertText", text="test@test.com"); time.sleep(0.4)
click(519, 195, 0.8)
s.send("Input.insertText", text="123456"); time.sleep(0.4)
shot("q2_form")
click(452, 295, 12); shot("q3_server")
click(292, 193, 0.6); click(292, 193, 8); shot("q4_map")
for i in range(6):
    click(713, 466, 0.9)
shot("q5_page8")
click(492, 393, 2.0); shot("q6_item")
print(time.strftime("%H:%M:%S"), "DONE", flush=True)
