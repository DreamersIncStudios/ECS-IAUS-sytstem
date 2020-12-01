using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Stats
{
    public class EnemyCharacter : PlayerCharacter
    {

        private void Update()
        {
            if (CurHealth <= 0) {
                Debug.Log("Dead");

             
            }
        }
 
    }
}
