using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class InstantDeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IHPScript>(out IHPScript hp))
        {
            hp.TakeDamage(math.INFINITY, this.gameObject, this.gameObject);
        }
    }
}