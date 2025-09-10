using System.Collections.Generic;
using Combinators;
using DreamersIncStudio.FactionSystem;
using Global.Component;
using Stats.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
// ReSharper disable Unity.BurstFunctionSignatureContainsManagedTypes

namespace AISenses.VisionSystems
{
    public partial class VisionTargetingUpdateGroup : ComponentSystemGroup
    {
        public VisionTargetingUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(1920, true);
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
        //    world.UpdateBodyIndexMap();
            state.Dependency = new TargetingVisionRayCastJob()
            {
                World = world,
                QuadrantMap = quadrantMultiHashMap,
                trams = query.ToComponentDataArray<LocalTransform>(Allocator.TempJob),
                stats = query.ToComponentDataArray<AIStat>(Allocator.TempJob),
                FactionsBuffer = SystemAPI.GetSingletonBuffer<Factions>(true)
      
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

    

        partial struct TargetingVisionRayCastJob : IJobEntity
        {
            [ReadOnly] public CollisionWorld World;
            [ReadOnly] public NativeParallelMultiHashMap<int, TargetQuadrantData> QuadrantMap;

            [ReadOnly] public NativeArray<AIStat> stats;
           [ReadOnly] public NativeArray<LocalTransform> trams;
           private const float CellEdgePadding = 50f;
          [ReadOnly] public DynamicBuffer<Factions> FactionsBuffer;
           
           void Execute(Entity entity, ref DynamicBuffer<ScanPositionBuffer> buffer, ref Vision vision,
               ref PhysicsInfo physicsInfo, in LocalTransform transform, in AITarget target)
           {
               buffer.Clear();
               var hashMapKey = TargetingQuadrantSystem.GetPositionHashMapKey(transform.Position);
               if (vision.HasTarget) return;

               var pos = transform.Position;


               var checkKeys = new List<int> { hashMapKey };
               AddAdjacentQuadrantKeys(pos, vision.ViewRadius, hashMapKey, checkKeys);
               var targets = GetAllTargetsInCheckList(checkKeys);

               var ctx = new TargetCtx(transform, vision,  target.FactionID, World,FactionsBuffer,  new CollisionFilter()
                   {
                       BelongsTo = ((1 << 11)),
                       CollidesWith = physicsInfo.CollidesWith.Value,
                       GroupIndex = 0
                   });
               
               var pred = PredChain
                   .Start(new IsAlive())
                   .And(new InRange())
                   .And(new InViewCone())
                   .And(new InViewRayCast())
                   .Build();
               var filteredTarget = pred.Test(targets, transform, in ctx);
               
               foreach (var targetQuadrantData in filteredTarget)
               {
                   buffer.Add(new ScanPositionBuffer()
                   {
                       target = new Target()
                       {
                           CanSee = true,
                           TargetInfo = targetQuadrantData.TargetInfo,
                           Entity = targetQuadrantData.Entity,
                           DistanceTo = targetQuadrantData.Distance,
                           LastKnownPosition = targetQuadrantData.Position,
                           
                       }

                   });
               }
               
           }

           private void AddAdjacentQuadrantKeys(float3 pos, float viewRadius, int baseKey, List<int> outKeys)
            {
                // Compute current quadrant indices
                var quadX = math.floor(pos.x / QuadrantCellSize);
                var quadZ = math.floor(pos.z / QuadrantCellSize);

                // X boundaries
                var xOffset = pos.x - (quadX * QuadrantCellSize);
                if (xOffset < viewRadius)
                {
                    outKeys.Add(baseKey - 1);
                }
                else if (xOffset > QuadrantCellSize - CellEdgePadding)
                {
                    outKeys.Add(baseKey + 1);
                }

                // Z boundaries
                var zOffset = pos.z - (quadZ * QuadrantCellSize);
                if (zOffset < viewRadius)
                {
                    outKeys.Add(baseKey - QuadrantYMultiplier);
                }
                else if (zOffset > QuadrantCellSize - CellEdgePadding)
                {
                    outKeys.Add(baseKey + QuadrantYMultiplier);
                }
                
            }

            private List<TargetQuadrantData> GetAllTargetsInCheckList(List<int> checkKeys)
            {
                var targets = new List<TargetQuadrantData>();
                foreach (var key in checkKeys)
                {
                    if (QuadrantMap.TryGetFirstValue(key, out var value, out var iterator))
                    {
                        do
                        {
                            targets.Add(value);
                        } while (QuadrantMap.TryGetNextValue(out value, ref iterator));
                    }
                }
                return targets;
            }
        }

    }
    public struct TargetQuadrantData
    {
        public Entity Entity;
        public float3 Position;
        public AITarget TargetInfo;
        public float Distance { get; set; }
    }

}
    

