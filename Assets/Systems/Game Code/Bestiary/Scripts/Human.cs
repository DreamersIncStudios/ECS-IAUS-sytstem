using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using Global.Component;
using AISenses;
//using IAUS.ECS.Component;
using DreamersInc.InflunceMapSystem;
using Unity.Entities;
//using DreamerInc.CombatSystem;
using Assets.Systems.Global.Function_Timer;
using DG.Tweening;
using Unity.Transforms;
using Unity.Mathematics;
//using MotionSystem.Archetypes;
using DreamersInc.ComboSystem.NPC;
using UnityEngine.AI;
using IAUS.ECS.Component;
using Components.MovementSystem;
using IAUS.ECS;

namespace BestiaryLibrary
{
    public static partial class BestiaryDB
    {
        public static Entity SpawnBasicHumanAndCreateEntityData(Vector3 Position, PhysicsInfo physicsInfo, string entityName = "")
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entityLink = createEntity(manager, entityName);
            GameObject spawnedGO = SpawnGO(manager, entityLink, Position, "NPCs/Combat/Human", 0);

            AddPhysics(manager, entityLink, spawnedGO, PhysicsShape.Capsule, physicsInfo);
            manager.SetComponentData(entityLink, new Translation { Value = spawnedGO.transform.position });
            manager.SetComponentData(entityLink, new Rotation { Value = spawnedGO.transform.rotation });
            AddTargetingAndInfluence(manager, entityLink,
                new AITarget()
                {
                    Type = TargetType.Character,
                    GetRace = Race.Human,
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
                new Perceptibility() {
                    visibilityStates = VisibilityStates.Visible,
                    movement = MovementStates.Stadning_Still,
                    noiseState = NoiseState.Normal
                }, new InfluenceComponent());
            AddMovementSystems(manager, entityLink, spawnedGO);

            spawnedGO.GetComponent<EnemyCharacter>().SetupDataEntity(manager, entityLink);
            spawnedGO.tag = "NPC";
            // spawnedGO.GetComponent<NPCCharacterController>().SetupDataEntity(entityLink);
            spawnedGO.GetComponent<NPCComboComponentAuthoring>().SetupDataEntity(entityLink);

            return entityLink;

        }


        public static void SpawnNPCGOandCreateDataEntity(Vector3 Pos, PhysicsInfo physicsInfo, AITarget Self, Perceptibility perceptibility, InfluenceComponent influence, List<AIStates> AIStatesAvailable, string entityName = "")
        {
            Utilities.GlobalFunctions.RandomPoint(Pos, 150.0f, out Vector3 spawnPos);
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entityLink = createEntity(manager, entityName);
            GameObject spawnedGO = SpawnGO(manager, entityLink, spawnPos, "NPCs/Non-Combat", 0);
            AddPhysics(manager, entityLink, spawnedGO, PhysicsShape.Capsule, physicsInfo);
            manager.SetComponentData(entityLink, new Translation { Value = spawnedGO.transform.position });
            manager.SetComponentData(entityLink, new Rotation { Value = spawnedGO.transform.rotation });

            AddTargetingAndInfluence(manager, entityLink, Self,
               new Vision()
               {
                   viewRadius = 55,
                   EngageRadius = 40,
                   ViewAngle = 360
               }, perceptibility,influence);
            AddMovementSystems(manager, entityLink, spawnedGO);
            manager.AddComponentData(entityLink, new IAUSBrain
            {
                factionID = Self.FactionID,
                Difficulty = Difficulty.Normal, //Todo Pull information from game master
                Attitude = Status.Normal,
                NPCLevel = NPCLevel.NPC
            });
            manager.AddBuffer<StateBuffer>(entityLink);
            DynamicBuffer<TravelWaypointBuffer> buffer = manager.AddBuffer<TravelWaypointBuffer>(entityLink);
            List<TravelWaypointBuffer> Waypoints = GetPoints(75, 5, Pos);
            foreach (var item in Waypoints)
            {
                buffer.Add(item);
            }



            NPCChararacter stats = spawnedGO.GetComponent<NPCChararacter>();
            stats.SetupDataEntity(manager,entityLink);
            AddAdditionalStates(Pos, Self, AIStatesAvailable, manager, entityLink, stats);

            manager.AddComponent<SetupBrainTag>(entityLink);

        }

       
    }
}
