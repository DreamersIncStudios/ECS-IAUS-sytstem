using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using DreamersInc.DamageSystem.Interfaces;
public class DamageCubeTester : MonoBehaviour
{
    public int damage = 25;
    public Element Element { get; private set; }

    public TypeOfDamage TypeOfDamage { get; private set; }

    public void OnTriggerEnter(Collider other)
    {
        IDamageable hit = other.GetComponent<IDamageable>();
        //Todo add Friend filter.
        if (hit != null)
        {
            hit.TakeDamage(25, TypeOfDamage, Element);
        }

        Debug.Log("Enter Damage Cube for testing");
    }
}

