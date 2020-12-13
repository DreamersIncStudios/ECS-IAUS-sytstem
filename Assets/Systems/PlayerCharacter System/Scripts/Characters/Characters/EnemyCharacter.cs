using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Stats
{
    public class EnemyCharacter : BaseCharacter
    {
        private void Start()
        {
            SetAttributeBaseValue(10, 300, 100, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20);
        }

    }
}
