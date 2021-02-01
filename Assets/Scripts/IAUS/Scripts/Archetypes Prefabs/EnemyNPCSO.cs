using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using UnityEngine.AI;
using IAUS.SO.Interfaces;
using Global.Component;
using IAUS.ECS2;
using Components.MovementSystem;
using IAUS.ECS2.Component;
using Stats;
using AISenses.Authoring;

namespace IAUS.SO {
    public class EnemyNPCSO : NPCSO, IEnemyNPC
    {
        public override void Spawn(Vector3 pos)
        {
            base.Spawn(pos);
            switch (GetInfluence.Level) 
            {
                case InfluenceSystem.Component.NPCLevel.Leader:
                    Leader lead =SpawnedGO.AddComponent<Leader>();
                    lead.influence = GetInfluence;
                    break;
            }
        }

    }
}