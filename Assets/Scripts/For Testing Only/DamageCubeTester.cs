using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
public class DamageCubeTester : MonoBehaviour
{
    public int damage = 25;

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EnemyCharacter>()) {
            EnemyCharacter EC = other.GetComponent<EnemyCharacter>();
            EC.DecreaseHealth(damage, 1, 0.1f);

            Debug.Log(EC.Name + "Enter Damage Cube for testing");
        }
    }
}
