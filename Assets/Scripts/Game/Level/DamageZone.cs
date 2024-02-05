using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class DamageZone : MonoBehaviour
{
    [SerializeField] public float damage;
    [SerializeField] public float timeBetweenDamage;
    [SerializeField] public Vector3 HalfOfBoxExtensions;


    private float lastDamageTime;
    private Collider[] colliders;


    private void OnEnable()
    {
        lastDamageTime = Time.time;
    }


    private void Update()
    {
        if (Time.time < lastDamageTime) return;
        lastDamageTime = Time.time + timeBetweenDamage;


        colliders = Physics.OverlapBox(transform.position, HalfOfBoxExtensions, Quaternion.identity);


        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<IHPScript>(out IHPScript hp))
            {
                hp.TakeDamage(damage);
            }
        }
    }
}
