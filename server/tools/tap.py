#!/usr/bin/env python3
"""Tap at stage coords via CDP touch events. usage: tap.py x y [shotname]"""
import sys, json, base64, time
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget

sx, sy = float(sys.argv[1]), float(sys.argv[2])
shotn = sys.argv[3] if len(sys.argv) > 3 else None
p = [t for t in jget("/json/list") if "41520" in t.get("url", "") and t["type"] == "page"][0]
s = Session(p["webSocketDebuggerUrl"]); s.send("Page.enable")
s.send("Emulation.setTouchEmulationEnabled", enabled=True, maxTouchPoints=1)
r = json.loads(s.send("Runtime.evaluate", expression="JSON.stringify(document.querySelector('ruffle-player').getBoundingClientRect())", returnByValue=True)["result"]["value"])
x = r["x"] + sx / 960.0 * r["width"]; y = r["y"] + sy / 560.0 * r["height"]
s.send("Input.dispatchTouchEvent", type="touchStart", touchPoints=[{"x": x, "y": y}])
time.sleep(0.12)
s.send("Input.dispatchTouchEvent", type="touchEnd", touchPoints=[])
print("tap", sx, sy, "->", round(x), round(y), flush=True)
time.sleep(1.2)
if shotn:
    z = s.send("Page.captureScreenshot", format="png")
    open("/opt/work/%s.png" % shotn, "wb").write(base64.b64decode(z["data"]))
    print("SHOT", shotn, flush=True)
