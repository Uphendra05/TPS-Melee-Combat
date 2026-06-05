using System;
using UnityEngine;

public interface IWeapon 
{
    

    event Action<Collider> OnHit;
    public void EnableHitbox();
    public void DisableHitbox();


}
