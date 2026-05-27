using UnityEngine;
using UnityEngine.Events;

public sealed class AN_Sample : BaseAnimationNotify
{
    public override void ExecuteEventNotify(Animator animator, UnityAction callback = null)
    {
        Debug.Log(animator.gameObject.name + "ATTACKING WITH CLAYMOREE !");
    }

    
}
