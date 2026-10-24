using System;
using System.Collections.Generic;
using FTRuntime;
using UnityEngine;

[DisallowMultipleComponent]
public class SwfClipFrameEvents : MonoBehaviour
{
    private sealed class FrameEvent
    {
        public int Frame;
        public bool Once;
        public Action Callback;
        public bool Fired;
    }

    private readonly List<FrameEvent> _events = new List<FrameEvent>();
    private SwfClip _clip;
    private SwfClipController _controller;
    private int _lastFrame = -1;
    private string _lastSequence;

    public void AddEvent(int frame, bool once, Action callback)
    {
        if (callback == null) return;
        _events.Add(new FrameEvent { Frame = frame, Once = once, Callback = callback });
    }

    public void ClearAllEvents() { _events.Clear(); _lastFrame = -1; }

    public void ResetAll()
    {
        for (int i = 0; i < _events.Count; i++) _events[i].Fired = false;
        _lastFrame = -1;
    }

    public void OnSequenceChanged() { ResetAll(); }

    private void Awake()
    {
        _clip = GetComponent<SwfClip>();
        _controller = GetComponent<SwfClipController>();
    }

    private void Update()
    {
        if (_clip == null) return;
        if (_controller != null && !_controller.isPlaying) return;

        string sequence = _clip.sequence;
        if (sequence != _lastSequence) { _lastSequence = sequence; OnSequenceChanged(); }

        int frame = _clip.currentFrame;
        if (frame == _lastFrame) return;
        _lastFrame = frame;

        for (int i = 0; i < _events.Count; i++)
        {
            FrameEvent frameEvent = _events[i];
            if (frameEvent.Frame != frame) continue;
            if (frameEvent.Once && frameEvent.Fired) continue;
            if (frameEvent.Once) frameEvent.Fired = true;
            try { frameEvent.Callback(); }
            catch (Exception exception) { Debug.LogException(exception, this); }
        }
    }
}
