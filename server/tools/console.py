#!/usr/bin/env python3
import sys, time, json
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget

p = [x for x in jget("/json/list") if "pinggy" in x["url"]][0]
s = Session(p["webSocketDebuggerUrl"])
s.ws.settimeout(5)
s.send("Runtime.enable")
s.send("Log.enable")
s.send("Page.enable")
s.send("Page.reload", ignoreCache=True)
t0 = time.time()
buf = []
while time.time() - t0 < 40:
    s.drain(1.0)
    buf = s.events
    for e in list(buf):
        if e.get("method") in ("Runtime.consoleAPICalled", "Runtime.exceptionThrown", "Log.entryAdded"):
            print(json.dumps(e)[:500])
            buf.remove(e)
        elif e.get("method") == "Page.loadEventFired":
            print("LOADED at", round(time.time() - t0, 1))
