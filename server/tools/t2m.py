#!/usr/bin/env python3
"""Verify touch->mouse-pointer shim on the 开始 button."""
import sys, time, base64, json
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
s.send("Page.navigate", url="http://127.0.0.1:41520/?t2m=1")

SHIM = """(function(){
  var host = null;
  function canvas(){
    host = document.querySelector('ruffle-player');
    return host && host.shadowRoot && host.shadowRoot.querySelector('canvas');
  }
  function fire(c, t, x, y, buttons){
    c.dispatchEvent(new PointerEvent(t,{pointerType:'mouse',pointerId:1,isPrimary:true,
      clientX:x,clientY:y,button:0,buttons:buttons,bubbles:true,cancelable:true,composed:true,view:window}));
  }
  document.addEventListener('touchstart', function(e){
    var c = canvas(); if(!c) return;
    var t = e.changedTouches[0];
    fire(c,'pointermove',t.clientX,t.clientY,0);
    fire(c,'pointerdown',t.clientX,t.clientY,1);
    e.preventDefault();
  }, true);
  document.addEventListener('touchend', function(e){
    var c = canvas(); if(!c) return;
    var t = e.changedTouches[0];
    fire(c,'pointermove',t.clientX,t.clientY,0);
    fire(c,'pointerup',t.clientX,t.clientY,0);
    e.preventDefault();
  }, true);
})();"""

time.sleep(45)
s.send("Runtime.evaluate", expression=SHIM)
def shot(p):
    r = s.send("Page.captureScreenshot", format="png")
    open(p, "wb").write(base64.b64decode(r["data"]))
    print("SHOT", p, flush=True)
shot("/opt/work/c1_before.png")
s.send("Input.dispatchTouchEvent", type="touchStart", touchPoints=[{"x": 490, "y": 440}])
time.sleep(0.12)
s.send("Input.dispatchTouchEvent", type="touchEnd", touchPoints=[])
time.sleep(10)
shot("/opt/work/c2_after.png")
