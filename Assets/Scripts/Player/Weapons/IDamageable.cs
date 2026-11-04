using UnityEngine;
using static UnityEngine.Analytics.IAnalytic;

public interface IDamageable 
{
    void TakeHit(in DamageInfo hit);
}

public struct DamageInfo
{
    public float damage;
    public Vector3 knockbackDirection;
    public float knockbackForce;
    public float stunDuration;
    public int hitReactionType;
    public Vector3 point;
    public Vector3 normal;
    public Transform attacker;
}