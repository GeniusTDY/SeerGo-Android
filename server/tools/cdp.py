#!/usr/bin/env python3
"""Minimal CDP client for the sandbox Chrome at 127.0.0.1:9222."""
import json, sys, time, urllib.request, ssl, base64, os
import websocket  # websocket-client

CDP = os.environ.get("CDP_URL", "http://127.0.0.1:9223")


def jget(path):
    with urllib.request.urlopen(CDP + path, timeout=10) as r:
        return json.loads(r.read().decode())


def new_page(url="about:blank"):
    req = urllib.request.Request(CDP + "/json/new?url=" + urllib.parse.quote(url, safe=""), method="PUT")
    with urllib.request.urlopen(req, timeout=10) as r:
        return json.loads(r.read().decode())


class Session:
    def __init__(self, ws_url):
        self.ws = websocket.create_connection(ws_url, timeout=15, suppress_origin=True)
        self.msg_id = 0
        self.events = []

    def send(self, method, **params):
        self.msg_id += 1
        mid = self.msg_id
        self.ws.send(json.dumps({"id": mid, "method": method, "params": params}))
        while True:
            raw = self.ws.recv()
            if not raw:
                continue
            m = json.loads(raw)
            if m.get("id") == mid:
                return m.get("result", {})
            self.events.append(m)

    def drain(self, timeout=0.5):
        self.ws.settimeout(timeout)
        try:
            while True:
                raw = self.ws.recv()
                if not raw:
                    break
                self.events.append(json.loads(raw))
        except Exception:
            pass
        self.ws.settimeout(15)
        return self.events
