#!/usr/bin/env python3
import json, sys, time, base64, os
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

URL = sys.argv[1] if len(sys.argv) > 1 else "http://127.0.0.1:41520/?v=3"
WAIT = float(sys.argv[2]) if len(sys.argv) > 2 else 25
SHOT = sys.argv[3] if len(sys.argv) > 3 else "/opt/tools/shot.png"

page = new_page("about:blank")
s = Session(page["webSocketDebuggerUrl"])
s.send("Runtime.enable")
s.send("Log.enable")
s.send("Network.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
s.send("Page.enable")
s.send("Page.navigate", url=URL)

t0 = time.time()
lines = []
while time.time() - t0 < WAIT:
    evs = s.drain(0.6)
    for e in evs:
        m = e.get("method")
        p = e.get("params", {})
        if m == "Runtime.consoleAPICalled":
            parts = []
            for a in p.get("args", []):
                parts.append(a.get("value") if "value" in a else a.get("description", a.get("type")))
            lines.append("[" + p.get("type", "?") + "] " + " ".join(str(x) for x in parts))
        elif m == "Runtime.exceptionThrown":
            d = p.get("exceptionDetails", {})
            lines.append("[exception] " + str(d.get("text")) + " " + str((d.get("exception") or {}).get("description", ""))[:400])
        elif m == "Log.entryAdded":
            ent = p.get("entry", {})
            lines.append("[log:" + ent.get("level", "?") + "] " + str(ent.get("text"))[:500])
        elif m == "Network.loadingFailed":
            lines.append("[netfail] " + str(p.get("errorText")) + " " + str(p.get("blockedReason")))
    s.events = []

for l in lines:
    print(l)

shot = s.send("Page.captureScreenshot", format="png")
with open(SHOT, "wb") as f:
    f.write(base64.b64decode(shot["data"]))
print("SHOT", SHOT)
