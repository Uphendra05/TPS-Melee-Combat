using UnityEngine;
using UnityEngine.Events;

public abstract class BaseAnimationNotifyState 
{
    public abstract void OnNotifyStart(Animator animator, float totalDuration, UnityAction callback = null);
    public abstract void OnNotifyTick(Animator animator, float deltaTime, UnityAction callback = null);
    public abstract void OnNotifyEnd(Animator animator, UnityAction callback = null);


}
