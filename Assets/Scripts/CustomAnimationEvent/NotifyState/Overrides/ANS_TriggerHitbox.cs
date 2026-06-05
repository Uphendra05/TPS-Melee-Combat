using UnityEngine;
using UnityEngine.Events;

public class ANS_TriggerHitbox : BaseAnimationNotifyState
{
    private WeaponManager weaponManager;

    public override void OnNotifyStart(Animator animator, float totalDuration, UnityAction callback = null)
    {
        if (weaponManager == null)
            weaponManager = animator.GetComponent<WeaponManager>();

        weaponManager.GetEquippedWeapon()?.EnableHitbox();
        Debug.Log("Inside trigger hitbox");
    }

    public override void OnNotifyTick(Animator animator, float deltaTime, UnityAction callback = null)
    {

    }

    public override void OnNotifyEnd(Animator animator, UnityAction callback = null)
    {
        weaponManager.GetEquippedWeapon()?.DisableHitbox();
        Debug.Log("outside trigger hitbox");

    }


}
