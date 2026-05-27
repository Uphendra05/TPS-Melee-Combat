using UnityEngine;
using UnityEngine.Events;

public sealed class ANS_SampleState : BaseAnimationNotifyState
{
   
    public override void OnNotifyStart(Animator animator, float totalDuration, UnityAction callback = null)
    {
        Debug.Log("INSIDE START : ATTACK STARTED");
    }

    public override void OnNotifyTick(Animator animator, float deltaTime, UnityAction callback = null)
    {
        Debug.Log("INSIDE TICK : ATTACKING");

    }

    public override void OnNotifyEnd(Animator animator, UnityAction callback = null)
    {
        Debug.Log("INSIDE END : ATTACK ENDED");

    }
}
