#!/usr/bin/env python3
"""连接到现有 smoke 页并检查状态"""
import sys, json, base64, time
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget

pages = [x for x in jget("/json/list") if x["type"] == "page"]
print("pages:", [p["url"][:60] for p in pages], flush=True)
p = pages[0]
s = Session(p["webSocketDebuggerUrl"]); s.ws.settimeout(90)
s.send("Runtime.enable")

def ev(e):
    return s.send("Runtime.evaluate", expression=e, returnByValue=True, awaitPromise=False).get("result", {}).get("value")

print("player:", ev("!!document.querySelector('ruffle-player')"), flush=True)
print("err:", repr(ev("document.getElementById('err').textContent")), flush=True)
print("rect:", ev("JSON.stringify((function(){var p=document.querySelector('ruffle-player');if(!p)return null;var q=p.getBoundingClientRect();return [q.x,q.y,q.width,q.height]})())"), flush=True)
z = s.send("Page.captureScreenshot", format="png")
open("/opt/work/smoke.png", "wb").write(base64.b64decode(z["data"]))
print("SHOT smoke.png", flush=True)
