using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using SpawnerSystem.Loot;
using Utilities.ECS;


namespace SpawnerSystem
{
    public class EnemyGOC : MonoBehaviour, IConvertGameObjectToEntity
    {

        Entity reference;
        public bool DestroyGO;

        [SerializeField] public List<ItemSpawnData> LootTable;
        public uint numOfDropitems =1;
        public List<ItemSpawnData> Dropped { get; set; }


        // add a clean up Destory Component tag and system
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var c1 = new EnemyTag() { Destory = false };
            dstManager.AddComponentData(entity, c1);
                       reference = entity;
        }

        public void Update()
        {
            if (DestroyGO)
                DestroyEnemy();
        }
        void DestroyEnemy()
        {
            EntityManager mgr = World.DefaultGameObjectInjectionWorld.EntityManager;
            spawnItemDropSpawnPoint(mgr);   
            
            mgr.AddComponentData(reference, new Destroytag() { delay = 0.0f });

            SpawnController.Instance.CountInScene--;
            Destroy(this.gameObject);

        }

        void spawnItemDropSpawnPoint(EntityManager MGR) {


            Entity entity = MGR.CreateEntity();
            MGR.AddComponentData(entity, new SpawnPointComponent()
            {
                Temporoary = true,
                SpawnPointID = 10000
            });
            MGR.AddComponentData(entity, new SelectADropTag() { NumOfDrops = numOfDropitems });

            DynamicBuffer<ItemSpawnData> Buffer = MGR.AddBuffer<ItemSpawnData>(entity);

            // Change to a custom input.

            foreach (ItemSpawnData loot in LootTable)
            {
                Buffer.Add(loot);
            }
            MGR.AddComponentData(entity, new ItemSpawnTag());
            MGR.AddComponent<CreateLootTableTag>(entity);
            var position = transform.TransformPoint(this.transform.position);
            MGR.AddComponentData(entity, new Translation() { Value = position });
            MGR.AddComponentData(entity, new LocalToWorld() { Value = transform.localToWorldMatrix});
            MGR.AddComponentData(entity, new ProbTotal() { probabilityTotalWeight = 0.0f });

            MGR.SetName(entity,"Loot Spawn Point");// This wont build why ?


        }

    }



}