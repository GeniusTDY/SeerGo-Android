#!/usr/bin/env python3
"""冒烟：清洁版 index.html 是否正常启动（标题页）"""
import sys, json, base64, time
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"]); s.ws.settimeout(60)
s.send("Page.enable"); s.send("Runtime.enable")
s.send("Network.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
s.send("Page.navigate", url="http://127.0.0.1:41520/?smoke=%d" % int(time.time()))

def ev(e):
    return s.send("Runtime.evaluate", expression=e, returnByValue=True).get("result", {}).get("value")

t0 = time.time()
ok = False
while time.time() - t0 < 60:
    try:
        if ev("!!document.querySelector('ruffle-player')"):
            ok = True
            break
    except Exception:
        pass
    time.sleep(2)
print("player:", ok, round(time.time() - t0), flush=True)
time.sleep(25)
print("err:", ev("document.getElementById('err').textContent"), flush=True)
print("rect:", ev("JSON.stringify((function(){var p=document.querySelector('ruffle-player');if(!p)return null;var q=p.getBoundingClientRect();return [q.x,q.y,q.width,q.height]})())"), flush=True)
z = s.send("Page.captureScreenshot", format="png")
open("/opt/work/smoke.png", "wb").write(base64.b64decode(z["data"]))
print("SHOT smoke.png", flush=True)
