using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Player;



public class InstantDeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<IHPScript>(out IHPScript hp))
        {
            hp.TakeDamage(math.INFINITY);
        }
    }
}
