using System;
using UnityEngine;

public class Weapon : MonoBehaviour, IWeapon
{

    public event Action<Collider> OnHit;

    private Collider _hitbox;

    private void Awake() => _hitbox = GetComponent<Collider>();

    public void EnableHitbox() => _hitbox.enabled = true;
    public void DisableHitbox() => _hitbox.enabled = false;


    private void OnTriggerEnter(Collider other)
    {
        OnHit?.Invoke(other);
    }
}
