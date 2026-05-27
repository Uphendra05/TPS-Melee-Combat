using UnityEngine;
using UnityEngine.Events;


public abstract class BaseAnimationNotify
{
    public abstract void ExecuteEventNotify(Animator animator, UnityAction callback = null);
   


}
