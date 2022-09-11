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
using Random = UnityEngine.Random;
using System.Linq;
using Unity.Physics;
using Unity.Physics.Authoring;

public  class NPCSpawn : MonoBehaviour
{
    static List<GameObject> Models;
    public static PhysicsCategoryTags belongsTo;
    public static PhysicsCategoryTags collideWith;
    bool loaded;
    public void LoadModals()
    {
        GameObject[] goLoaded = Resources.LoadAll("Players", typeof(GameObject)).Cast<GameObject>().ToArray();
        foreach (var go in goLoaded)
        {
            Models.Add(go);
        }
        loaded = true;
    }

    public static void SpawnTowerAndCreateDataEntity(Vector3 Pos, string entityName = "")
    {
        Utilities.GlobalFunctions.RandomPoint(Pos, 150.0f, out Vector3 spawnPos);

        EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype npcDataArch = manager.CreateArchetype(
           typeof(Translation),
           typeof(Rotation),
           typeof(LocalToWorld),
           typeof(NPCStats),
           typeof(IAUSBrain),
           typeof(AITarget),
           typeof(InfluenceComponent),
           typeof(StateBuffer),
           typeof(Perceptibility),
           typeof(Vision),
           typeof(ScanPositionBuffer),
           typeof(CopyTransformFromGameObject),
           typeof(PhysicsCollider),
           typeof(PhysicsWorldIndex)
           );

        Entity npcDataEntity = manager.CreateEntity(npcDataArch);
        if (entityName != string.Empty)
            manager.SetName(npcDataEntity, entityName);
        else
            manager.SetName(npcDataEntity, "NPC Data");

        int cnt = Random.Range(0, Models.Count - 1);
        GameObject spawnedGO = GameObject.Instantiate(Models[cnt], spawnPos, Quaternion.identity);
        UnityEngine.BoxCollider col = spawnedGO.GetComponent<UnityEngine.BoxCollider>();
        BlobAssetReference<Unity.Physics.Collider> spCollider = Unity.Physics.BoxCollider.Create(new BoxGeometry()
        {
            Size = col.size,
            Center = col.center,

        }, new CollisionFilter()
        {
            BelongsTo = belongsTo.Value,
            CollidesWith = collideWith.Value,
            GroupIndex = 0
        }
);
        manager.SetComponentData(npcDataEntity, new PhysicsCollider()
        { Value = spCollider });


        manager.SetComponentData(npcDataEntity, new Translation { Value = spawnPos });


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
            factionID = 2,
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
            FactionID = 2,
            CanBeTargetByPlayer = true,
            Type = TargetType.Character,
        });
        manager.SetComponentData(npcDataEntity, new Perceptibility
        {
            visibilityStates = VisibilityStates.Visible,
            movement = MovementStates.Stadning_Still,
            noiseState = NoiseState.Normal
        });

        manager.SetComponentData(npcDataEntity, new InfluenceComponent
        {
            factionID = 2,
            Protection = 5,
            Threat = 5

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

        EnemyCharacter stats = spawnedGO.GetComponent<EnemyCharacter>();
        stats.SetupDataEntity(npcDataEntity);

        manager.AddComponentObject(npcDataEntity, spawnedGO.GetComponent<UnityEngine.BoxCollider>());

        manager.AddComponentObject(npcDataEntity, spawnedGO.transform);
        manager.AddComponentObject(npcDataEntity, spawnedGO.GetComponent<Rigidbody>());

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
