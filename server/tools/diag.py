#!/python3
import sys, time, json, base64
sys.path.insert(0, "/opt/tools")
from cdp import Session, new_page, jget

p = new_page("about:blank")
s = Session(p["webSocketDebuggerUrl"])
s.ws.settimeout(200)
s.send("Page.enable"); s.send("Network.enable"); s.send("Runtime.enable"); s.send("Log.enable")
s.send("Network.setCacheDisabled", cacheDisabled=True)
s.send("Page.navigate", url="http://127.0.0.1:41520/?clean=4")

def drain():
    s.ws.settimeout(0.4)
    try:
        while True:
            raw = s.ws.recv()
            if not raw: break
            m = json.loads(raw)
            s.events.append(m)
    except Exception:
        pass
    s.ws.settimeout(200)

def click(sx, sy):
    r = s.send("Runtime.evaluate", expression="JSON.stringify(document.querySelector('ruffle-player').getBoundingClientRect())", returnByValue=True)
    rx = json.loads(r["result"]["value"])
    x = rx["x"] + sx / 960.0 * rx["width"]; y = rx["y"] + sy / 560.0 * rx["height"]
    s.send("Input.dispatchMouseEvent", type="mouseMoved", x=x, y=y, button="none", buttons=0)
    time.sleep(0.1)
    s.send("Input.dispatchMouseEvent", type="mousePressed", x=x, y=y, button="left", buttons=1, clickCount=1)
    time.sleep(0.1)
    s.send("Input.dispatchMouseEvent", type="mouseReleased", x=x, y=y, button="left", buttons=0, clickCount=1)

def scan(tag):
    out = []
    for m in s.events:
        meth = m.get("method", "")
        if meth == "Runtime.consoleAPICalled":
            args = m["params"].get("args", [])
            txt = " ".join(str(a.get("value", a.get("description", ""))) for a in args)
            if m["params"].get("type") in ("error", "warning") or "rror" in txt or "AVM2" in txt or "Exception" in txt:
                out.append(txt[:300])
        elif meth == "Runtime.exceptionThrown":
            out.append("EXC " + str(m["params"].get("exceptionDetails", {}).get("text"))[:200])
        elif meth == "Log.entryAdded":
            e = m["params"]["entry"]
            if e.get("level") in ("error", "warning"):
                out.append(e.get("text", "")[:300])
    print(tag, len(out), flush=True)
    for t in out[-25:]:
        print("   ", t.replace("\n", " ")[:260], flush=True)

time.sleep(50)
scan("before-start")
click(480, 517)
time.sleep(45)
scan("after-start")
shot = s.send("Page.captureScreenshot", format="png")
open("/opt/work/x4_after_start.png", "wb").write(base64.b64decode(shot["data"]))
print("SHOT ok", flush=True)
