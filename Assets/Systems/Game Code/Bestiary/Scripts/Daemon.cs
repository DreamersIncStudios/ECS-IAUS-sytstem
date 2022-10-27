using AISenses;
using DreamersInc.ComboSystem.NPC;
using DreamersInc.InflunceMapSystem;
using Global.Component;
using IAUS.ECS.Component;
using Stats;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace BestiaryLibrary
{
    public static partial class BestiaryDB
    {
        public static Entity SpawnBasicDaemonAndCreateEntityData(Vector3 Position, PhysicsInfo physicsInfo, out GameObject spawnedGO, string entityName = "")
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entityLink = createEntity(manager, entityName);
            spawnedGO = SpawnGO(manager, entityLink, Position, "NPCs/Combat/Daemons");
            AddPhysics(manager, entityLink, spawnedGO, PhysicsShape.Capsule, physicsInfo);
            AddMovementSystems(manager, entityLink, spawnedGO);
            spawnedGO.tag = "Enemy NPC";
            spawnedGO.GetComponent<EnemyCharacter>().SetupDataEntity(manager, entityLink);
            spawnedGO.GetComponent<NPCComboComponentAuthoring>().SetupDataEntity(entityLink);
            AddTargetingAndInfluence(manager, entityLink,
                    new AITarget()
                    {
                        Type = TargetType.Character,
                        GetRace = Race.Daemon,
                        MaxNumberOfTarget = 5,
                        CanBeTargetByPlayer = true,
                        CenterOffset = new float3(0, 1, 0)
                    },
                    new Vision()
                    {
                        viewRadius = 55,
                        EngageRadius = 40,
                        ViewAngle = 360
                    },
                    new Perceptibility()
                    {
                        visibilityStates = VisibilityStates.Visible,
                        movement = MovementStates.Stadning_Still,
                        noiseState = NoiseState.Normal
                    }, new InfluenceComponent());
            return entityLink;
        }

        public static void SpawnDameonGruntandCreateDataEntity(Vector3 Pos, PhysicsInfo physicsInfo, AITarget Self, Perceptibility perceptibility, InfluenceComponent influence, List<AIStates> AIStatesAvailable, string entityName = "")
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var entityLink = SpawnBasicDaemonAndCreateEntityData(Pos, physicsInfo, out GameObject spawnedGO, entityName);
            EnemyCharacter stats = spawnedGO.GetComponent<EnemyCharacter>();
            stats.SetupDataEntity(manager, entityLink);
            manager.AddBuffer<StateBuffer>(entityLink);
            DynamicBuffer<TravelWaypointBuffer> buffer = manager.AddBuffer<TravelWaypointBuffer>(entityLink);
            List<TravelWaypointBuffer> Waypoints = GetPoints(75, 5, Pos);
            foreach (var item in Waypoints)
            {
                buffer.Add(item);
            }
        }
    }
}
