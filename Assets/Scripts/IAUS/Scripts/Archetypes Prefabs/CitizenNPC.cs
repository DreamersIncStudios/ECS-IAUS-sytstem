using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using AISenses.Authoring;
using Global.Component;
using AISenses;
using IAUS.ECS;
using Components.MovementSystem;
using IAUS.ECS.Component;
using DreamersInc.InflunceMapSystem;
using UnityEngine.AI;
using Unity.Entities;

namespace IAUS.NPCSO
{
    [System.Serializable]
    //TODO Write Custom Property Drawers
    public class CitizenNPC {
        [Range(1, 500)]
        public int Count;
        public GameObject Model;
        public AITarget Self;
        public List<AIStates> AIStatesAvailable;
        public PatrolBuilderData GetPatrol;
        public WaitBuilderData GetWait;
        public Movement AIMove;
        public Vision GetVision;
        public InfluenceComponent GetInfluence;

        public void Spawn(Vector3 pos)
        {
            for (int i = 0; i <= Count; i++)
            {
                GameObject spawnedGO = Object.Instantiate(Model, pos, Quaternion.identity);
                spawnedGO.AddComponent<NavMeshAgent>();
                spawnedGO.AddComponent<ConvertToEntity>().ConversionMode = ConvertToEntity.Mode.ConvertAndInjectGameObject;
                BaseAIAuthoringSO aiAuthoring = spawnedGO.AddComponent<BaseAIAuthoringSO>();
                //AIAuthoring.faction = getFaction;
                aiAuthoring.Self = Self;
                aiAuthoring.GetAttackType = new List<AttackTypeInfo>();
                aiAuthoring.movement = AIMove;
                foreach (AIStates state in AIStatesAvailable)
                {
                    switch (state)
                    {
                        case AIStates.Patrol:
                            aiAuthoring.AddPatrol = true;
                            aiAuthoring.buildPatrol = GetPatrol;
                            spawnedGO.AddComponent<WaypointCreation>();
                            break;
                        case AIStates.Wait:
                            aiAuthoring.AddWait = true;
                            aiAuthoring.waitBuilder = GetWait;
                            break;
                    }

                }

                AISensesAuthoring Senses = spawnedGO.AddComponent<AISensesAuthoring>();
                Senses.Vision = true;
                Senses.VisionData = GetVision;
                //Senses.Hearing = true;
                //Senses.HearingData = new Hearing();
                NPCChararacter npcStat = spawnedGO.AddComponent<NPCChararacter>();
                npcStat.SetAttributeBaseValue(10, 300, 100, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20);
                npcStat.Name = NPCUtility.GetNameFile();
                aiAuthoring.factionID = 3; //  TODO Set up later;
                aiAuthoring.GetInfluence = GetInfluence;
            }
        }
    }
    
}