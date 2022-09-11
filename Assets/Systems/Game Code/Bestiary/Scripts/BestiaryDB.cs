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
namespace BestiaryLibrary
{
    public static class BestiaryDB 
    {
        static List<GameObject> LoadModels(string path) {
            List < GameObject> modelFromResources = new List<GameObject> ();
            GameObject[] goLoaded = Resources.LoadAll(path, typeof(GameObject)).Cast<GameObject>().ToArray();
            foreach (var go in goLoaded)
            {
                modelFromResources.Add(go);
            }
            Debug.Log(modelFromResources.Count);
            return modelFromResources;
        }

      public static Entity SpawnTowerAndCreateEntityData(Vector3 Position, out GameObject goRef,string entityName = "") {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            EntityArchetype npcDataArch = manager.CreateArchetype(
               typeof(Translation),
               typeof(Rotation),
               typeof(LocalToWorld),
               typeof(EnemyStats),
               typeof(IAUSBrain),
               typeof(AITarget),
               typeof(InfluenceComponent),
               typeof(Wait),
               typeof(GatherResourcesState),
               typeof(AttackTargetState),
               typeof(AttackTypeInfo),
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
                manager.SetName(npcDataEntity, "Tower Data");
            var Models = LoadModels("NPCs/Combat");
            int cnt = Random.Range(0, Models.Count);
            #region GameObject Setup
            GameObject spawnedGO = GameObject.Instantiate(Models[0], Position+ new Vector3(0, 1.525f, 0), Quaternion.identity);
           goRef = spawnedGO;
            manager.SetComponentData(npcDataEntity, new Translation { Value = Position });
            manager.AddComponentObject(npcDataEntity, spawnedGO.transform);
            if(spawnedGO.GetComponent<Animator>())
            manager.AddComponentObject(npcDataEntity, spawnedGO.GetComponent<Animator>());

            #endregion

            //Todo Change Later Box Collider????
            #region Physics
            UnityEngine.BoxCollider col = spawnedGO.GetComponent<UnityEngine.BoxCollider>();
            BlobAssetReference<Unity.Physics.Collider> spCollider = Unity.Physics.BoxCollider.Create(new BoxGeometry()
            {
                Center= col.center,
                Size= col.size,
                Orientation = quaternion.identity,

            }, new CollisionFilter()
            {
                BelongsTo = (1 >> 10),
                CollidesWith = (1 >> 11),
                GroupIndex = 0
            });
            manager.SetComponentData(npcDataEntity, new PhysicsCollider()
            { Value = spCollider });
            manager.AddComponentObject(npcDataEntity, spawnedGO.GetComponent<UnityEngine.CapsuleCollider>());
            manager.AddComponentObject(npcDataEntity, spawnedGO.GetComponent<Rigidbody>());

            #endregion

            #region Stats
            EnemyCharacter stats = spawnedGO.GetComponent<EnemyCharacter>();
            stats.SetupDataEntity(npcDataEntity);


            #endregion
            #region IAUS
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
            manager.SetComponentData(npcDataEntity, new IAUSBrain
            {
                factionID = 2,
                Difficulty = Difficulty.Normal, //Todo Pull information from game master
                Attitude = Status.Normal,
                NPCLevel = NPCLevel.NPC
            });
            manager.SetComponentData(npcDataEntity, new Vision
            {
                ViewAngle = 360,
                viewRadius = 45,
                EngageRadius = 20,

            });
            manager.SetComponentData(npcDataEntity, new Wait
            {
                StartTime = 1.0f
            });



            #endregion
            manager.AddComponent<SetupBrainTag>(npcDataEntity);

            return npcDataEntity;

        }
    }
}
