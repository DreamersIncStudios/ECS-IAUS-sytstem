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

namespace BestiaryLibrary
{
    public static partial class BestiaryDB
    {
        public static Entity SpawnTowerAndCreateEntityData(Vector3 Position, PhysicsInfo physicsInfo, string entityName = "")
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entityLink = createEntity(manager, entityName);
            GameObject spawnedGO = SpawnGO(manager, entityLink, Position, "NPCs/Combat/Tower");
           
            AddPhysics(manager, entityLink, spawnedGO,PhysicsShape.Box, physicsInfo);
            AddTargetingAndInfluence(manager, entityLink, new AITarget(), new Vision() {
                viewRadius = 55, EngageRadius = 40, ViewAngle = 360
            },
                            new Perceptibility()
                            {
                                visibilityStates = VisibilityStates.Visible,
                                movement = MovementStates.Stadning_Still,
                                noiseState = NoiseState.Normal
                           }, new InfluenceComponent()
            );
            #region Stats
            EnemyCharacter stats = spawnedGO.GetComponent<EnemyCharacter>();
            stats.SetupDataEntity(manager,entityLink);
            StaticObjectControllerAuthoring controller = spawnedGO.GetComponent<StaticObjectControllerAuthoring>();
            controller.SetupControllerEntityData(manager, entityLink);

            #endregion
            return entityLink;

        }
   
        public static Entity SpawnTowerAndCreateEntityData(Vector3 Position, PhysicsInfo physicsInfo, out GameObject spawnedGO, string entityName = "")
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entityLink = createEntity(manager, entityName);
            spawnedGO = SpawnGO(manager, entityLink, Position, "NPCs/Combat/Tower");

            AddPhysics(manager, entityLink, spawnedGO, PhysicsShape.Box, physicsInfo);
            AddTargetingAndInfluence(manager, entityLink,new AITarget(), new Vision()
            {
                viewRadius = 55,
                EngageRadius = 40,
                ViewAngle = 360
            }, new Perceptibility()
            {
                visibilityStates = VisibilityStates.Visible, movement = MovementStates.Stadning_Still, noiseState = NoiseState.Normal
            }, new InfluenceComponent()
            );
            #region Stats
            EnemyCharacter stats = spawnedGO.GetComponent<EnemyCharacter>();
            stats.SetupDataEntity(manager, entityLink);
            StaticObjectControllerAuthoring controller = spawnedGO.GetComponent<StaticObjectControllerAuthoring>();
            controller.SetupControllerEntityData(manager, entityLink);

            #endregion
            return entityLink;

        }
        public static Entity SpawnTowerAndCreateEntityDataWithVFX(Vector3 Position, PhysicsInfo physicsInfo, string entityName = "")
        {
          //  VFXManager.Instance.PlayVFX(6, Position, 6);
            Entity temp = new Entity();
            FunctionTimer.Create(() => {
                temp = SpawnTowerAndCreateEntityData(Position + Vector3.down * 5, physicsInfo, out GameObject spawnGO, entityName);
                spawnGO.transform.DOMoveY(Position.y + 1.2f, 3);
            }, 2, "Spawn Tower");

            return temp;
        }

    }


}