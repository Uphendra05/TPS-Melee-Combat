using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "AttackData/Weapons")]
public class WeaponSO : ScriptableObject
{
    public WeaponType weaponType;
    public DamageType damageType;
    public AnimatorOverrideController attackAnimation;
    public float damage;
    public float damageMultiplier;
}
