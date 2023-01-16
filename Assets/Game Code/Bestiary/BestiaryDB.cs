using Global.Component;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DreamersInc.BestiarySystem
{
    public class BestiaryDB : MonoBehaviour
    {
        //Text file or ScriptableOjects;
        static public List<CreatureInfo> Creatures;
        static public bool IsLoaded { get; private set; }
        private static void ValidateDatabase() {
            if (Creatures == null || !IsLoaded)
            {
                Creatures = new List<CreatureInfo>();
                IsLoaded = false;
            }
            else { 
                IsLoaded= true;
            }
        }

        public static void LoadDatabase(bool ForceLoad = false) {

            if (IsLoaded && !ForceLoad)
                return;
            Creatures = new List<CreatureInfo>();
            CreatureInfo[] SO = Resources.LoadAll<CreatureInfo>(@"Creatures");
            foreach (var item in SO)
            {
                if(!Creatures.Contains(item))
                    Creatures.Add(item);
            }
            IsLoaded = true;
        }

        public static void ClearDatabase()
        {
            IsLoaded = false;
            Creatures.Clear();

        }
        public static CreatureInfo GetCreature(uint id)
        {
            ValidateDatabase();
            LoadDatabase();
            foreach(CreatureInfo creature in Creatures)
            {
                 if(creature.ID== id) return creature;

            }
            return null;
        }
        public static void SpawnCreature(uint ID) { 
            var info = GetCreature(ID);
            var go = Instantiate(info.Prefab);
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity testing = CreateEntity(manager, info.Name);
            AddPhysics(manager, testing, go, PhysicsShape.Capsule, info.PhysicsInfo);
        }
        private static Entity CreateEntity(EntityManager manager, string entityName = "")
        {

            EntityArchetype baseEntityArch = manager.CreateArchetype(
              typeof(WorldTransform),
              typeof(LocalTransform),
              typeof(LocalToWorld)
              );
            Entity baseDataEntity = manager.CreateEntity(baseEntityArch);
            if (entityName != string.Empty)
                manager.SetName(baseDataEntity, entityName);
            else
                manager.SetName(baseDataEntity, "NPC Data");

            return baseDataEntity;
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
                    break;
            }
            manager.AddSharedComponent(entityLink, new PhysicsWorldIndex());
            manager.AddComponentData(entityLink, new PhysicsCollider()
            { Value = spCollider });
            manager.AddComponentData(entityLink, new PhysicsInfo
            {
                BelongsTo = physicsInfo.BelongsTo,
                CollidesWith = physicsInfo.CollidesWith
            });
        }

        private void Awake()
        {
            LoadDatabase();
            Debug.Log(Creatures.Count);
        }
        enum PhysicsShape { Box, Capsule, Sphere, Cyclinder, Custom }
    }

}