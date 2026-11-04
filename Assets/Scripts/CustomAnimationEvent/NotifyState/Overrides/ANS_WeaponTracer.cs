using UnityEngine;
using UnityEngine.Events;

public class ANS_WeaponTracer : BaseAnimationNotifyState
{
    public float damage = 10f;
    public float knockbackForce = 5f;
    public float stunDuration = 0.3f;
    public int hitReactionType;

    public override void OnNotifyStart(Animator animator, float totalDuration, UnityAction callback = null)
    {
        if (!animator.TryGetComponent(out WeaponManager wm) || wm.currentWeapon == null) return;
        wm.currentWeapon.Begin(new SwingData
        {
            damage = damage,
            knockbackForce = knockbackForce,
            stunDuration = stunDuration,
            hitReactionType = hitReactionType
        }, animator.transform);
    }

    public override void OnNotifyTick(Animator animator, float deltaTime, UnityAction callback = null) { }

    public override void OnNotifyEnd(Animator animator, UnityAction callback = null)
    {
        if (animator.TryGetComponent(out WeaponManager wm) && wm.currentWeapon != null)
            wm.currentWeapon.End();

    }


}
