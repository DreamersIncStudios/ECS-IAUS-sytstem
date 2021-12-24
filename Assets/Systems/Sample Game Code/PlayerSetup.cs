using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;

namespace SampleGame.Setup
{
    public class PlayerSetup : CharacterSetup
       {
        PlayerCharacter PC;
        public string CharacterName;

        private void Awake()
        {
            PC = this.GetComponent<PlayerCharacter>();
        }

        void Start()
        {
            PC.Name = CharacterName;
            StatsUpdate(PC);
        }

        
    }
}