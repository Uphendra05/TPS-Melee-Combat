using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AnimationEventPlayer : MonoBehaviour
{
    private Animator _animator;
    private AnimationEventSO _currentData;
    private float _previousTime = 0f;
    private HashSet<int> _activeStates = new();

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Play(AnimationEventSO data)
    {
        StopAllActiveStates();
        _currentData = data;
        _previousTime = 0f;
    }

    public void Tick(float normalizedTime)
    {
        if (_currentData == null) return;

        float duration = _currentData.clip.length;
        float currentTime = normalizedTime * duration;

        for (int i = 0; i < _currentData.events.Count; i++)
        {
            var ev = _currentData.events[i];

            if (ev.IsState())
                ProcessState(i, ev, currentTime);
            else
                ProcessNotify(i, ev, currentTime);
        }

        _previousTime = currentTime;
    }

    private void ProcessNotify(int index, AnimationEventEntry ev, float currentTime)
    {
        if (ev.notify == null) return;

        bool crossed = _previousTime < ev.triggerTime && currentTime >= ev.triggerTime;
        if (crossed)
            ev.notify.ExecuteEventNotify(_animator);
    }

    private void ProcessState(int index, AnimationEventEntry ev, float currentTime)
    {
        //if (ev.notifyState == null) return;

        //bool wasInside = _previousTime >= ev.startTime && _previousTime < ev.endTime;
        //bool isInside = currentTime >= ev.startTime && currentTime < ev.endTime;

        //if (!wasInside && isInside)
        //{
        //    _activeStates.Add(index);
        //    ev.notifyState.OnNotifyStart(_animator,);
        //}
        //else if (wasInside && isInside)
        //{
        //    ev.notifyState.OnNotifyTick(_animator, Time.deltaTime);
        //}
        //else if (wasInside && !isInside)
        //{
        //    _activeStates.Remove(index);
        //    ev.notifyState.OnNotifyEnd(_animator);
        //}
    }

    public void StopAllActiveStates()
    {
        if (_currentData == null) return;
        foreach (int index in _activeStates)
            _currentData.events[index].notifyState?.OnNotifyEnd(_animator);
        _activeStates.Clear();
        _previousTime = 0f;
    }
}
