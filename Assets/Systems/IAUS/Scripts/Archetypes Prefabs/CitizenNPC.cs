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
using System.Threading.Tasks;
using System;

namespace IAUS.NPCScriptableObj
{
    [System.Serializable]
    //TODO Write Custom Property Drawers
    public class CitizenNPC
    {
        [Range(1, 500)]
        public int Count;
        public GameObject Model;
        public AITarget Self;
        public List<AIStates> AIStatesAvailable;
        public MovementBuilderData GetMovement;
        public WaitBuilderData GetWait;
        public Movement AIMove;
        public Vision GetVision;
        public InfluenceComponent GetInfluence;

        public async void Spawn(Vector3 pos)
        {
            for (int i = 0; i <= Count; i++)
            {
                Utilities.GlobalFunctions.RandomPoint(pos, 5.0f, out Vector3 Spos);
                GameObject spawnedGO = GameObject.Instantiate(Model, Spos, Quaternion.identity);

                Self = new AITarget()
                {
                    FactionID = GetInfluence.factionID,
                    Type = TargetType.Character,
                    CanBeTargetByPlayer = false
                };


                if (!spawnedGO.GetComponent<NavMeshAgent>())
                    spawnedGO.AddComponent<NavMeshAgent>();

                spawnedGO.AddComponent<ConvertToEntity>().ConversionMode = ConvertToEntity.Mode.ConvertAndInjectGameObject;
                
                BaseAIAuthoringSO aiAuthoring = spawnedGO.AddComponent<BaseAIAuthoringSO>();
                NPCChararacter npcStat = spawnedGO.AddComponent<NPCChararacter>();
                npcStat.SetAttributeBaseValue(10, 300, 100, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20);

                //AIAuthoring.faction = getFaction;
                aiAuthoring.Self = Self;
                aiAuthoring.GetAttackType = new List<AttackTypeInfo>();
                aiAuthoring.movement = AIMove;
                foreach (AIStates state in AIStatesAvailable)
                {
                    switch (state)
                    {
                        case AIStates.Traverse:
                            aiAuthoring.AddTraverse = true;
                            aiAuthoring.buildMovement = GetMovement;
                            var adder = spawnedGO.AddComponent<WaypointCreation>();
                            await Task.Delay(TimeSpan.FromSeconds(1));
                            adder.CreateWaypoints(GetMovement.Range, GetMovement.NumberOfStops, false);

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
               
                npcStat.Name = NPCUtility.GetNameFile();
                aiAuthoring.factionID = 3; //  TODO Set up later;
                aiAuthoring.GetInfluence = GetInfluence;
                aiAuthoring.SetupSystem();

                await Task.Delay(TimeSpan.FromSeconds(5f));
         

            }
        }
    }

}