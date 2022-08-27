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
using Unity.Transforms;
using Utilities;
using Unity.Mathematics;
using Object = UnityEngine.Object;

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

        EntityArchetype citizen;
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

        public void SpawnDataEntity(Vector3 Pos,string entityName = "") {
            Utilities.GlobalFunctions.RandomPoint(Pos, 5.0f, out Vector3 Spos);

            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityArchetype npcDataArch = manager.CreateArchetype(
               typeof(Translation),
               typeof(Rotation),
               typeof(LocalToWorld),
               typeof(NPCStats),
               typeof(IAUSBrain),
               typeof(AITarget),
               typeof(InfluenceComponent),
               typeof(Wait),
               typeof(Traverse),
               typeof(Movement),
               typeof(TravelWaypointBuffer),
               typeof(StateBuffer),
                               typeof(Vision),
               typeof(ScanPositionBuffer),
               typeof(CompanionGO)
               );

            Entity npcDataEntity = manager.CreateEntity(npcDataArch);
            if (entityName != string.Empty)
                manager.SetName(npcDataEntity, entityName);
            else
                manager.SetName(npcDataEntity, "NPC Data");
            manager.SetComponentData(npcDataEntity, new Translation { Value = Pos });


            manager.SetComponentData(npcDataEntity, new Wait
            {
                StartTime = 1.0f
            });

            manager.SetComponentData(npcDataEntity, new Vision
            {
                ViewAngle = 160,
                viewRadius = 45,
                EngageRadius = 20,

            }); ;
            manager.SetComponentData(npcDataEntity, new IAUSBrain
            {
                factionID = Self.FactionID,
                Difficulty = Difficulty.Normal, //Todo Pull information from game master
                Attitude = Status.Normal,
                NPCLevel = NPCLevel.NPC
            });
            manager.SetComponentData(npcDataEntity, new Traverse
            {
                BufferZone = 0.75f,
                _coolDownTime = 0,
                NumberOfWayPoints = 5
            });
            DynamicBuffer<TravelWaypointBuffer> buffer = manager.GetBuffer<TravelWaypointBuffer>(npcDataEntity);
            List<TravelWaypointBuffer> Waypoints = GetPoints(75, 5, Pos);
            foreach (var item in Waypoints)
            {
                buffer.Add(item);
            }

            manager.SetComponentData(npcDataEntity, new AITarget
            {
                FactionID = Self.FactionID,
                CanBeTargetByPlayer = true,
                Type = TargetType.Character,
            });

            GameObject spawnedGO = GameObject.Instantiate(Model, Spos, Quaternion.identity);
            manager.SetComponentData(npcDataEntity, new CompanionGO
            {
                GOCompanion = spawnedGO
            });

            manager.SetComponentData(npcDataEntity, new InfluenceComponent
            { 
                factionID = Self.FactionID,
                Protection = 2,
                Threat = 0

            });
            if (spawnedGO.GetComponent<NavMeshAgent>() == null)
            {
                NavMeshAgent agent = spawnedGO.AddComponent<NavMeshAgent>();
            }

            manager.SetComponentData(npcDataEntity, new Movement
            {
                CanMove = true,
                //Todo pull info from data 
                MovementSpeed = 10,
                StoppingDistance = 1.0f,
                Acceleration = 5
            });

            NPCChararacter stats = spawnedGO.AddComponent<NPCChararacter>();
            stats.SetupNPCData(npcDataEntity, 10);
          
            foreach (AIStates state in AIStatesAvailable)
            {
                switch (state)
                {
                    case AIStates.Retreat:
                        var data = new RetreatCitizen();
                        data._coolDownTime = 2.5f;
                        data.RetreatRange = stats.GetPrimaryAttribute((int)AttributeName.Speed).AdjustBaseValue * 5.0f / 100f;
                        data.CrowdMin = 4;
                        data.ThreatThreshold = 4;
                        manager.AddComponentData(npcDataEntity, data);
                        break;
                }

            }

            manager.AddComponent<SetupBrainTag>(npcDataEntity);

        }
        static List<TravelWaypointBuffer> GetPoints(uint range, uint NumOfPoints, Vector3 pos, bool Safe = true)
        {

            List<TravelWaypointBuffer> Points = new List<TravelWaypointBuffer>();
            while (Points.Count < NumOfPoints)
            {
                if (GlobalFunctions.RandomPoint(pos, range, out Vector3 position))
                {
                    Points.Add(new TravelWaypointBuffer()
                    {
                        WayPoint = new Waypoint()
                        {
                            Position = (float3)position,
                            Point = new AITarget()
                            {
                                Type = TargetType.Location,
                                FactionID = -1
                            },

                            TimeToWaitatWaypoint = UnityEngine.Random.Range(5, 10)
                        }
                    }
                 );
                }
            }
            return Points;

        }

    }
    public partial class SyncCompanionGOPosition : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, ref LocalToWorld local, CompanionGO GO) => {
                EntityManager.SetComponentData(entity, new Translation { Value = GO.GOCompanion.transform.position });

            });
            Entities.ForEach((Entity entity, ref Rotation rot, CompanionGO GO) => {
                EntityManager.SetComponentData(entity, new Rotation { Value = GO.GOCompanion.transform.rotation });

            });
            Entities.ForEach((Entity entity, ref EntityHasDiedTag tag, CompanionGO go) => {
                Debug.Log("Play Death Animation");
                Object.Destroy(go.GOCompanion, .5f);
                EntityManager.DestroyEntity(entity);
            });


        }
    }
}