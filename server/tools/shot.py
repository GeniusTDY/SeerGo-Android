import json, sys, time, base64
sys.path.insert(0, "/opt/tools")
from cdp import Session, jget
WAIT = float(sys.argv[1]) if len(sys.argv) > 1 else 10
SHOT = sys.argv[2] if len(sys.argv) > 2 else "/opt/tools/shot.png"
pages = [p for p in jget("/json/list") if "41520" in p.get("url", "")]
page = pages[0]
s = Session(page["webSocketDebuggerUrl"])
time.sleep(WAIT)
shot = s.send("Page.captureScreenshot", format="png")
open(SHOT, "wb").write(base64.b64decode(shot["data"]))
print("SHOT", SHOT)
