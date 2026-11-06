using UnityEngine;
using UnityEngine.Events;

public class ANS_WeaponFX : BaseAnimationNotifyState
{

    public override void OnNotifyStart(Animator animator, float totalDuration, UnityAction callback = null)
    {
        if (!animator.TryGetComponent(out WeaponManager wm) || wm.currentWeapon == null) return;
        wm.ActivateWeaponHitFX();

    }

    public override void OnNotifyTick(Animator animator, float deltaTime, UnityAction callback = null)
    {


    }

    public override void OnNotifyEnd(Animator animator, UnityAction callback = null)
    {
        if (!animator.TryGetComponent(out WeaponManager wm) || wm.currentWeapon == null) return;
        wm.DeactivateWeaponHitFX();
    }

   

   
}
