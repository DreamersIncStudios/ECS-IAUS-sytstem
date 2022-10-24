using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using AISenses.Authoring;
using Global.Component;
using AISenses;
using IAUS.ECS;
//using IAUS.ECS.Component;
using DreamersInc.InflunceMapSystem;
using UnityEngine.AI;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Linq;
using Unity.Physics;
//using DreamerInc.CombatSystem;
using Assets.Systems.Global.Function_Timer;
using DG.Tweening;
using Components.MovementSystem;
using DreamersInc.ComboSystem;
using IAUS.ECS.Component;
using Utilities;
using System;

namespace BestiaryLibrary
{
    public static partial class BestiaryDB
    {
        static List<GameObject> LoadModels(string path) {
            List<GameObject> modelFromResources = new List<GameObject>();
            GameObject[] goLoaded = Resources.LoadAll(path, typeof(GameObject)).Cast<GameObject>().ToArray();
            foreach (var go in goLoaded)
            {
                modelFromResources.Add(go);
            }
            return modelFromResources;
        }

        private static Entity createEntity(EntityManager manager, string entityName = "")
        {

            EntityArchetype baseEntityArch = manager.CreateArchetype(
              typeof(Translation),
              typeof(Rotation),
              typeof(LocalToWorld),
              typeof(CopyTransformFromGameObject)
              );
            Entity baseDataEntity = manager.CreateEntity(baseEntityArch);
            if (entityName != string.Empty)
                manager.SetName(baseDataEntity, entityName);
            else
                manager.SetName(baseDataEntity, "NPC Data");

            return baseDataEntity;
        }
        private static GameObject SpawnGO(EntityManager manager, Entity linkEntity, Vector3 Position, string modelPath = "", int cnt =-1)
        {
            var Models = LoadModels(modelPath);
            cnt =cnt == -1? Random.Range(0, Models.Count): cnt;
            GameObject spawnedGO = GameObject.Instantiate(Models[cnt], Position, Quaternion.identity);
            manager.SetComponentData(linkEntity, new Translation { Value = Position });
            manager.AddComponentObject(linkEntity, spawnedGO.transform);
            if (spawnedGO.GetComponent<Animator>())
                manager.AddComponentObject(linkEntity, spawnedGO.GetComponent<Animator>());
            manager.AddComponentObject(linkEntity, spawnedGO.GetComponentInChildren<Renderer>());

            return spawnedGO;
        }

        private static void AddPhysics(EntityManager manager, Entity entityLink, GameObject spawnedGO, PhysicsShape shape, PhysicsInfo physicsInfo)
        {
            BlobAssetReference<Unity.Physics.Collider> spCollider = new BlobAssetReference<Unity.Physics.Collider>();
            switch (shape)
            {
                case PhysicsShape.Capsule:
                    UnityEngine.CapsuleCollider col = spawnedGO.GetComponent<UnityEngine.CapsuleCollider>();
                    spCollider = Unity.Physics.CapsuleCollider.Create(new CapsuleGeometry()
                    {
                        Radius = col.radius,
                        Vertex0 = col.center + new Vector3(0, col.height, 0),
                        Vertex1 = new float3(0, 0, 0)

                    }, new CollisionFilter()
                    {
                        BelongsTo = physicsInfo.BelongsTo.Value,
                        CollidesWith = physicsInfo.CollidesWith.Value,
                        GroupIndex = 0
                    });

                    manager.AddComponentObject(entityLink, spawnedGO.GetComponent<UnityEngine.CapsuleCollider>());
                    manager.AddComponentObject(entityLink, spawnedGO.GetComponent<Rigidbody>());

                    break;
                case PhysicsShape.Box:
                    UnityEngine.BoxCollider box = spawnedGO.GetComponent<UnityEngine.BoxCollider>();
                    spCollider = Unity.Physics.BoxCollider.Create(new BoxGeometry()
                    {
                        Center = box.center,
                        Size = box.size,
                        Orientation = quaternion.identity,

                    }, new CollisionFilter()
                    {
                        BelongsTo = physicsInfo.BelongsTo.Value,
                        CollidesWith = physicsInfo.CollidesWith.Value,
                        GroupIndex = 0
                    });
                    manager.AddComponentData(entityLink, new PhysicsCollider()
                    { Value = spCollider });
                    manager.AddComponentObject(entityLink, spawnedGO.GetComponent<UnityEngine.BoxCollider>());

                    break;
            }
            manager.AddSharedComponentData(entityLink, new PhysicsWorldIndex());
            manager.AddComponentData(entityLink, new PhysicsCollider()
            { Value = spCollider });
            manager.AddComponentData(entityLink, new PhysicsInfo
            {
                BelongsTo = physicsInfo.BelongsTo,
                CollidesWith = physicsInfo.CollidesWith
            });
        }

        private static void AddTargetingAndInfluence(EntityManager manager, Entity entityLink, AITarget aiTarget, Vision visionData, Perceptibility perceptibility, InfluenceComponent influence)
        {
            manager.AddComponentData(entityLink, aiTarget);
            manager.AddComponentData(entityLink, influence);
            manager.AddComponentData(entityLink, visionData);
            manager.AddComponentData(entityLink, perceptibility);
            manager.AddBuffer<ScanPositionBuffer>(entityLink);

       
        }

        private static void AddMovementSystems(EntityManager em, Entity entityLink, GameObject spawnedGO)
        {
            em.AddComponentData(entityLink, new Movement()
            {
                CanMove = true,
                //Todo pull info from data 
                MovementSpeed = 10,
                StoppingDistance = 1.0f,
                Acceleration = 5
            });

            em.AddComponentObject(entityLink, spawnedGO.GetComponent<NavMeshAgent>());
        }

        private static void AddCombat(EntityManager em, Entity entityLink)
        {
            em.AddComponentData(entityLink, new Command()
            {
                InputQueue = new Queue<AnimationTrigger>(),
                BareHands = false,
                WeaponIsEquipped = true
            });
        }

        private static void AddAdditionalStates(Vector3 Pos,AITarget Self, List<AIStates> AIStatesAvailable, EntityManager manager, Entity entityLink, BaseCharacter stats)
        {
            foreach (AIStates state in AIStatesAvailable)
            {
                switch (state)
                {
                    case AIStates.Retreat:

                        manager.AddComponentData(entityLink, new RetreatCitizen() {
                            FactionMemberID = Self.FactionID,
                       _coolDownTime = 2.5f,
                        RetreatRange = stats.GetPrimaryAttribute((int)AttributeName.Speed).AdjustBaseValue * 5.0f / 100f,
                      CrowdMin = 4,
                       ThreatThreshold = 4
                });
                        break;
                    case AIStates.Traverse:
                        manager.AddComponentData(entityLink, new Traverse
                        {
                            BufferZone = 0.75f,
                            _coolDownTime = 5,
                            NumberOfWayPoints = 5
                        });
                        DynamicBuffer< TravelWaypointBuffer > buffer = manager.AddBuffer<TravelWaypointBuffer>(entityLink);
                        List<TravelWaypointBuffer> Waypoints = GetPoints(75, 5, Pos);
                        break;
                    case AIStates.Wait:
                        manager.AddComponentData(entityLink, new Wait
                        {
                            StartTime = 1.0f
                        });
                        break;
                }

            }
        }

        private static List<TravelWaypointBuffer> GetPoints(uint range, uint NumOfPoints, Vector3 pos, bool Safe = true)
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
        enum PhysicsShape { Box, Capsule, Sphere,  Cyclinder, Custom}
    }
}
