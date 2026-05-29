using System;
using UnityEngine;

public class TriggerCollisionEvent : MonoBehaviour
{

    public Action<Collider> OnHit;

    private void OnTriggerEnter(Collider other)
    {
        OnHit?.Invoke(other);
    }


}
