using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;

namespace SampleGame.Setup
{
    public class EnemySetup : CharacterSetup
    {
        EnemyCharacter EnemyStats;

        private void Awake()
        {
            EnemyStats = this.GetComponent<EnemyCharacter>();
        }


        // Start is called before the first frame update
        void Start()
        {
            EnemyStats.Name = CharClass.title.ToString();
            StatsUpdate(EnemyStats);
        }

    

    }
}