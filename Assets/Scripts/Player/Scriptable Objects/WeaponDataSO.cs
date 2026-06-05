using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "AttackData/Weapon Data")]
public class WeaponDataSO : BaseAnimationEventSO
{
    public WeaponType weaponType;
    public DamageType damageType;
    public float damage;
    public float damageMultiplier;
    public GameObject weaponModel;
}
