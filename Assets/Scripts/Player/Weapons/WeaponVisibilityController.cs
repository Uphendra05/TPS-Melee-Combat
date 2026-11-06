using UnityEngine;

public class WeaponVisibilityController : MonoBehaviour
{
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Animator animator;

    private bool weaponVisible;

    private void Start()
    {
        weaponManager = GetComponent<WeaponManager>();  
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        bool isAttacking = state.IsTag("LightAttack");

        if (isAttacking && !weaponVisible)
        {
            weaponManager.WeaponFXDissolveOut();
            weaponVisible = true;
        }
        else if (!isAttacking && weaponVisible)
        {
            weaponManager.WeaponFXDissolveIn();
            weaponVisible = false;
        }
    }
}
