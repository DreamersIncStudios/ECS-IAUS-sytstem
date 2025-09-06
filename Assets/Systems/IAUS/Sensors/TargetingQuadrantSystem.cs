using System.Runtime.CompilerServices;
using Combinators;
using Global.Component;
using Stats.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Properties;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace AISenses.VisionSystems
{
    public partial class VisionTargetingUpdateGroup : ComponentSystemGroup
    {
        public VisionTargetingUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(132, true);
        }

    }

    [UpdateInGroup(typeof(VisionTargetingUpdateGroup))]
    public partial struct TargetingQuadrantSystem : ISystem
    {
     
        private NativeParallelMultiHashMap<int, TargetQuadrantData> quadrantMultiHashMap;
        private const int QuadrantYMultiplier = 1000;
        private const int QuadrantCellSize = 50;
        private EntityQuery query;
    
        private static int GetPositionHashMapKey(float3 position)
        {
            return (int)(Mathf.Floor(position.x / QuadrantCellSize) +
                         (QuadrantYMultiplier * Mathf.Floor(position.z / QuadrantCellSize)));
        }

        public int GetEntityCountInHashMap(NativeParallelMultiHashMap<int, TargetQuadrantData> quadrantMap,
            int hashMapKey)
        {
            var count = 0;
            if (!quadrantMap.TryGetFirstValue(hashMapKey, out _,
                    out var iterator)) return count;
            do
            {
                count++;
            } while (quadrantMap.TryGetNextValue(out _, ref iterator));

            return count;
        }

     

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            quadrantMultiHashMap = new NativeParallelMultiHashMap<int, TargetQuadrantData>(0, Allocator.Persistent);
            query = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[]
                {
                    ComponentType.ReadWrite(typeof(LocalTransform)), ComponentType.ReadWrite(typeof(AITarget)) 
                    ,ComponentType.ReadWrite(typeof(AIStat))
                }
            });
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            quadrantMultiHashMap.Dispose();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            UpdateQuadrantHashMap(ref state);

            state.EntityManager.CompleteDependencyBeforeRO<PhysicsWorldSingleton>();
            var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            state.Dependency = new TargetingVisionRayCastJob()
            {
                World = world,
                QuadrantMap = quadrantMultiHashMap,
                trams = query.ToComponentDataArray<LocalTransform>(Allocator.TempJob),
                stats = query.ToComponentDataArray<AIStat>(Allocator.TempJob)
      
            }.ScheduleParallel(state.Dependency);
        }

        void UpdateQuadrantHashMap(ref SystemState systemState)
        {

            if (query.CalculateEntityCount() != quadrantMultiHashMap.Capacity)
            {
                quadrantMultiHashMap.Clear();
                quadrantMultiHashMap.Capacity = query.CalculateEntityCount() + 1;
            }

            new SetQuadrantDataHashMapJob()
            {
                QuadrantMap = quadrantMultiHashMap.AsParallelWriter()
            }.ScheduleParallel(query);
        }

        [BurstCompile]

        partial struct SetQuadrantDataHashMapJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int, TargetQuadrantData>.ParallelWriter QuadrantMap;

            private void Execute(Entity entity, [ReadOnly] in LocalTransform transform, in AITarget target)
            {
                var hashMapKey = GetPositionHashMapKey(transform.Position);
                QuadrantMap.Add(hashMapKey, new TargetQuadrantData
                {
                    Entity = entity,
                    Position = transform.Position,
                    TargetInfo = target
                });
            }
        }

        public struct TargetQuadrantData
        {
            public Entity Entity;
            public float3 Position;
            public AITarget TargetInfo;
        }

        [BurstCompile]
        partial struct TargetingVisionRayCastJob : IJobEntity
        {
            [ReadOnly] public CollisionWorld World;
            [ReadOnly] public NativeParallelMultiHashMap<int, TargetQuadrantData> QuadrantMap;

            [ReadOnly] public NativeArray<AIStat> stats;
           [ReadOnly] public NativeArray<LocalTransform> trams;
            void Execute(Entity entity, ref DynamicBuffer<ScanPositionBuffer> buffer, ref Vision vision,
                ref PhysicsInfo physicsInfo,
                in LocalTransform transform)
            {
                buffer.Clear();
                var hashMapKey = TargetingQuadrantSystem.GetPositionHashMapKey(transform.Position);
                
                var pred = PredChain
                    .Start(new InRange())
                    .And(new IsAlive())
                    .Build();
                var ctx = new TargetCtx(transform.Position, 100, new CollisionFilter());

                for (var index = 0; index < stats.Length; index++)
                {
                    if (pred.Test(stats[index],trams[index], in ctx)) 
                        Debug.Log("Hit");
                }
            }
        }

    }

}
    

