#!/usr/bin/env python3
"""Verify HUD shows ox/oy for a synthetic touch."""
import sys, time, base64
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"])
s.ws.settimeout(200)
s.send("Page.enable"); s.send("Network.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
s.send("Emulation.setTouchEmulationEnabled", enabled=True, maxTouchPoints=1)
bs = Session(jget("/json/version")["webSocketDebuggerUrl"])
bs.send("Target.activateTarget", targetId=page["id"])
s.send("Page.navigate", url="http://127.0.0.1:41520/?hud=1")
time.sleep(20)
s.send("Input.dispatchTouchEvent", type="touchStart", touchPoints=[{"x": 490, "y": 440}])
time.sleep(0.12)
s.send("Input.dispatchTouchEvent", type="touchEnd", touchPoints=[])
time.sleep(1)
print(s.send("Runtime.evaluate", expression="document.querySelector('div[style*=fixed]').textContent", returnByValue=True).get("result", {}).get("value"))
print(s.send("Runtime.evaluate", expression="(function(){var c=document.querySelector('ruffle-player').shadowRoot.querySelector('canvas');var r=c.getBoundingClientRect();return JSON.stringify({x:r.x,y:r.y,w:r.width,h:r.height,cw:c.clientWidth,ch:c.clientHeight})})()", returnByValue=True).get("result", {}).get("value"))
