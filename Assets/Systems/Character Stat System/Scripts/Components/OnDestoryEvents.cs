using DreamersInc;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Stats
{
    public sealed class OnDestoryEvents : MonoBehaviour
    {
        public uint ExpOnDeath { get; private set; }

        public void SetExp(uint exp) { 
            ExpOnDeath = exp;
        }
        private void OnDestroy()
        {
            if (Application.isPlaying) { 
                IncreasePlayerEXP();
            }
        }

        void IncreasePlayerEXP() {
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            manager.CreateEntityQuery(typeof(BaseCharacterComponent)).TryGetSingletonEntity<Player_Control>(out var player);
            manager.GetComponentData<BaseCharacterComponent>(player).AddExp(ExpOnDeath);
        }
    }
}