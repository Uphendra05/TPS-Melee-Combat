using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "AttackData/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    public WeaponType weaponType;
    public DamageType damageType;
    public AnimationClip attackAnimation;
    public float damage;
    public float damageMultiplier;
    public GameObject weaponModel;
}
