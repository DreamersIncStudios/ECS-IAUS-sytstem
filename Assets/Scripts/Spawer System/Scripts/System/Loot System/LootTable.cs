using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
namespace SpawnerSystem.Loot
{

    public struct CreateLootTableTag : IComponentData
    {
        [HideInInspector]
        public float probabilityTotalWeight;
    }
    public struct SelectADropTag : IComponentData
    {
        public uint NumOfDrops;
    }

    public struct ProbTotal : IComponentData
    {
        public float probabilityTotalWeight;
    }

    [UpdateAfter(typeof(LootSystem))]
    public class DropSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {

            //rewrite code
            Entities.ForEach((Entity entity, ref SelectADropTag DropInfo, ref ProbTotal prob, ref LocalToWorld Pos, ref ItemSpawnTag Tag) =>
            {

                int Scnt = new int();
                DynamicBuffer<ItemSpawnData> Buffer = EntityManager.GetBuffer<ItemSpawnData>(entity);
                List<ItemSpawnData> Dropped = new List<ItemSpawnData>();
                for (int cnt = 0; cnt < DropInfo.NumOfDrops; cnt++)
                {
                    float pickedNumber = Random.Range(0, prob.probabilityTotalWeight);
                    // Find an item whose range contains pickedNumber
                    foreach (ItemSpawnData Drop in Buffer)
                    {
                        
                        // If the picked number matches the item's range, return item
                        if (pickedNumber > Drop.spawnData.probabilityRangeFrom && pickedNumber < Drop.spawnData.probabilityRangeTo)
                        {
                            Dropped.Add(Drop);
                            goto top;
                        }

                    }
                    top:
                    Scnt++; // this need to be refactor for a do check ;
                }
                foreach (ItemSpawnData Item in Dropped)
                {
                    Vector3 point;
                    if (RandomPoint(Pos.Position, Tag.spawnrange, out point))
                    {
                        ItemDatabase.GetItem(Item.spawnData.SpawnID).Spawn(point);
                    }
                }
                 PostUpdateCommands.DestroyEntity(entity);

            });
              
        }
        bool RandomPoint(Vector3 center, float range, out Vector3 result)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }
            result = Vector3.zero;
            return false;
        }
    }


    public class LootSystem : JobComponentSystem
    {
        public EndSimulationEntityCommandBufferSystem end;

        protected override void OnCreate()
        {
            base.OnCreate();
            end = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            EntityCommandBuffer.Concurrent CommandBuffer = end.CreateCommandBuffer().ToConcurrent();
            JobHandle CreateLootTable = Entities
                .WithNativeDisableParallelForRestriction(CommandBuffer)
                .ForEach((Entity entity, int nativeThreadIndex, DynamicBuffer<ItemSpawnData> DropItems, ref CreateLootTableTag Enemy, ref ProbTotal prob) =>
            {
                float currentProbabilityWeightMaximum = 0f;


                for (int cnt = 0; cnt < DropItems.Length; cnt++)
                {
                    ItemSpawnData Drop = DropItems[cnt];
                    if (Drop.spawnData.probabilityWeight <= 0)
                    {
                        Debug.LogWarning("Loot Drop Item Not set up");
                        Drop.spawnData.probabilityWeight = 0f;
                    }
                    else
                    {
                        Drop.spawnData.probabilityRangeFrom = currentProbabilityWeightMaximum;
                        currentProbabilityWeightMaximum += Drop.spawnData.probabilityWeight;
                        Drop.spawnData.probabilityRangeTo = currentProbabilityWeightMaximum;
                    }
                    prob.probabilityTotalWeight = currentProbabilityWeightMaximum;

                    DropItems[cnt] = Drop;
                }


                for (int cnt = 0; cnt < DropItems.Length; cnt++)
                {
                    ItemSpawnData Drop = DropItems[cnt];
                    Drop.spawnData.probabilityPercent = ((Drop.spawnData.probabilityWeight) / prob.probabilityTotalWeight) * 100;
                    DropItems[cnt] = Drop;

                }
                CommandBuffer.RemoveComponent<CreateLootTableTag>(nativeThreadIndex, entity);


            }).Schedule(inputDeps);

            end.AddJobHandleForProducer(CreateLootTable);
            return CreateLootTable;
        }

    }
}
