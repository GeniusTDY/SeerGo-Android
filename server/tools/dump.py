import json, sys, time, base64
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget
WAIT = float(sys.argv[1]) if len(sys.argv) > 1 else 3
SHOT = sys.argv[2] if len(sys.argv) > 2 else ""
pages = [p for p in jget("/json/list") if "41520" in p.get("url", "")]
page = pages[0]
s = Session(page["webSocketDebuggerUrl"])
s.send("Runtime.enable"); s.send("Log.enable")
time.sleep(WAIT)
evs = s.drain(1.0)
for e in evs:
    m = e.get("method"); p = e.get("params", {})
    if m == "Runtime.consoleAPICalled":
        parts = []
        for a in p.get("args", []):
            parts.append(a.get("value") if "value" in a else a.get("description", a.get("type")))
        print("[" + p.get("type","?") + "]", " ".join(str(x) for x in parts)[:500])
    elif m == "Runtime.exceptionThrown":
        d = p.get("exceptionDetails", {})
        print("[exception]", str(d.get("text"))[:200], str((d.get("exception") or {}).get("description",""))[:400])
    elif m == "Log.entryAdded":
        ent = p.get("entry", {})
        print("[log:" + ent.get("level","?") + "]", str(ent.get("text"))[:400])
if SHOT:
    shot = s.send("Page.captureScreenshot", format="png")
    open(SHOT,"wb").write(base64.b64decode(shot["data"]))
    print("SHOT", SHOT)
