using Global.Component;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using RaycastHit = Unity.Physics.RaycastHit;

namespace AISenses.VisionSystems
{
    public partial class VisionTargetingUpdateGroup : ComponentSystemGroup
    {
        public VisionTargetingUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(15, true);
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

        public int GetEntityCountInHashMap(NativeParallelMultiHashMap<int, TargetQuadrantData> quadrantMap, int hashMapKey)
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

        private static void DebugDrawQuadrant(float3 position)
        {
            Vector3 lowerLeft = new(Mathf.Floor(position.x / QuadrantCellSize) * QuadrantCellSize,
                (QuadrantYMultiplier * Mathf.Floor(position.z / QuadrantCellSize) * QuadrantCellSize));
            Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+1, +0, +0) * QuadrantCellSize);
            Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(+0, +0, +1) * QuadrantCellSize);
            Debug.DrawLine(lowerLeft + new Vector3(+1, +0, +0) * QuadrantCellSize,
                lowerLeft + new Vector3(+1, +0, +1) * QuadrantCellSize);
            Debug.DrawLine(lowerLeft + new Vector3(+0, +0, +1) * QuadrantCellSize,
                lowerLeft + new Vector3(+0, +0, +0) * QuadrantCellSize);
            Debug.Log(GetPositionHashMapKey(position) + "" + position);

        }
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            quadrantMultiHashMap = new NativeParallelMultiHashMap<int, TargetQuadrantData>(0, Allocator.Persistent);
            query = new EntityQueryBuilder(Allocator.TempJob).WithAll<LocalTransform, AITarget>().Build(ref state);
        }
        [BurstCompile]
       public void OnDestroy(ref SystemState state)
        {
            quadrantMultiHashMap.Dispose();
        }
       
       [BurstCompile]
      public  void OnUpdate(ref SystemState state)
        {
            UpdateQuadrantHashMap(ref state);

            state.EntityManager.CompleteDependencyBeforeRO<PhysicsWorldSingleton>();
            var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            world.UpdateBodyIndexMap();
            state.Dependency = new TargetingVisionRayCastJob()
            {
                World = world,
                QuadrantMap = quadrantMultiHashMap
            }.ScheduleParallel(state.Dependency);
        }
      void UpdateQuadrantHashMap(ref SystemState systemState)
      {

          if (query.CalculateEntityCount() != quadrantMultiHashMap.Capacity)
          {
              quadrantMultiHashMap.Clear();
              quadrantMultiHashMap.Capacity = query.CalculateEntityCount()+1;
          }

          new SetQuadrantDataHashMapJob()
              { QuadrantMap = quadrantMultiHashMap.AsParallelWriter() }.ScheduleParallel(query);
      }

        [BurstCompile]
    
        public partial struct SetQuadrantDataHashMapJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int, TargetQuadrantData>.ParallelWriter QuadrantMap;

            private void Execute(Entity entity, [ReadOnly] in LocalTransform transform,in AITarget target)
            {
                var hashMapKey = GetPositionHashMapKey(transform.Position);
                QuadrantMap.Add(hashMapKey, new TargetQuadrantData
                {
                    Entity = entity,
                    Position = transform.Position,
                    TargetInfo =  target
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

            void Execute(Entity entity, ref DynamicBuffer<ScanPositionBuffer> buffer, ref Vision vision, ref PhysicsInfo physicsInfo,
                in LocalTransform transform)
            {
                buffer.Clear();
                var hashMapKey = TargetingQuadrantSystem.GetPositionHashMapKey(transform.Position);
                FindTargets(hashMapKey,entity,buffer,vision,transform,physicsInfo);
                FindTargets(hashMapKey+1,entity,buffer,vision,transform,physicsInfo);
                FindTargets(hashMapKey-1,entity,buffer,vision,transform,physicsInfo);
                FindTargets(hashMapKey+QuadrantYMultiplier,entity,buffer,vision,transform,physicsInfo);
                FindTargets(hashMapKey-QuadrantYMultiplier,entity,buffer,vision,transform,physicsInfo);
                FindTargets(hashMapKey+1+QuadrantYMultiplier,entity,buffer,vision,transform,physicsInfo);
                FindTargets(hashMapKey-1+QuadrantYMultiplier,entity,buffer,vision,transform,physicsInfo);      
                FindTargets(hashMapKey+1-QuadrantYMultiplier,entity,buffer,vision,transform,physicsInfo);
                FindTargets(hashMapKey-1-QuadrantYMultiplier,entity,buffer,vision,transform,physicsInfo);

            }

            void FindTargets(int hashMapKey,Entity entity,DynamicBuffer<ScanPositionBuffer> buffer, Vision vision, LocalTransform transform, PhysicsInfo physicsInfo)
            {
                if (QuadrantMap.TryGetFirstValue(hashMapKey, out var quadrantData, out var iterator))
                {
                    do
                    {
                        if(quadrantData.Entity.Equals(entity))
                            continue;
                        var dist = Vector3.Distance(transform.Position, quadrantData.Position);
                        if (dist < 30)
                        {
                            //Todo add visibility check at later date 
                            buffer.Add(new ScanPositionBuffer()
                            {
                                target = new Target()
                                {
                                    CanSee = true,
                                    DistanceTo = dist,
                                    LastKnownPosition = quadrantData.Position,
                                    TargetInfo = quadrantData.TargetInfo,
                                    Entity = quadrantData.Entity
                                },
                                dist = dist

                            });
                        }

                        if (!(dist < vision.ViewRadius)) continue;
                        var dirToTarget = ((Vector3)quadrantData.Position -
                                           (Vector3)(transform.Position + new float3(0, 1, 0))).normalized;
                        if (!(Vector3.Angle(transform.Forward(), dirToTarget) < vision.ViewAngle / 2.0f)) continue;
                        var raycastInput = new RaycastInput()
                        {
                            Start = transform.Position + new float3(0, 1, 0) + transform.Forward() * 3f,
                            End = quadrantData.Position + quadrantData.TargetInfo.CenterOffset,
                            Filter = new CollisionFilter()
                            {
                                BelongsTo = ((1 << 10)),
                                CollidesWith = physicsInfo.CollidesWith.Value,
                                GroupIndex = 0
                            }
                        };
                        if (!World.CastRay(raycastInput, out RaycastHit raycastHit)) continue;
                        if (raycastHit.Entity.Equals(quadrantData.Entity))
                        {

                            buffer.Add(new ScanPositionBuffer()
                            {
                                target = new Target()
                                {
                                    CanSee = true,
                                    DistanceTo = dist,
                                    LastKnownPosition = quadrantData.Position,
                                    TargetInfo = quadrantData.TargetInfo,
                                    Entity = quadrantData.Entity
                                },
                                dist = dist

                            });
                        }
                    } while (QuadrantMap.TryGetNextValue(out quadrantData, ref iterator));
                }

            }
        }
    }
    
}
    

