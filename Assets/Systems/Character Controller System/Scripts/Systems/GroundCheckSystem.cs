using DreamersInc;
using MotionSystem.Components;
using MotionSystem.Systems;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

namespace MotionSystem
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(AnimatorUpdate))]
    public partial struct GroundCheckSystem : ISystem
    {
        private NativeParallelMultiHashMap<int, QuadrantData> quadrantMultiHashMap;
        private const int quadrantYMultiplier = 1000;
        private const int quadrantCellSize = 100;
        public EntityQuery query;
        struct QuadrantData
        {
            public Entity entity;
            public float3 position;
        }

        static int GetPositionHashMapKey(float3 position)
        {
            return (int)(Mathf.Floor(position.x / quadrantCellSize) + (quadrantYMultiplier * Mathf.Floor(position.y / quadrantCellSize)));
        }
        int GetEntityCountInHashMap(NativeParallelMultiHashMap<int, QuadrantData> quadrantMap, int hashMapKey)
        {
            int count = 0;
            if (quadrantMap.TryGetFirstValue(hashMapKey, out QuadrantData quadrantData, out NativeParallelMultiHashMapIterator<int> iterator))
            {
                do
                {
                    count++;
                }
                while (quadrantMap.TryGetNextValue(out quadrantData, ref iterator));
            }
            return count;
        }

        public void OnCreate(ref SystemState state)
        {
            quadrantMultiHashMap = new NativeParallelMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
            query = state.GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(LocalTransform)), ComponentType.ReadWrite(typeof(CharControllerE)),
            ComponentType.ReadWrite(typeof(Animator))}
            });
        }


        public void OnDestroy(ref SystemState state)
        {
            quadrantMultiHashMap.Dispose();
       
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (query.CalculateEntityCount() > quadrantMultiHashMap.Capacity)
            {
                quadrantMultiHashMap.Clear();
                quadrantMultiHashMap.Capacity = query.CalculateEntityCount();

                new SetQuadrantDataHashMapJob() { quadrantMap = quadrantMultiHashMap.AsParallelWriter() }.ScheduleParallel(query);
            }

            if (SystemAPI.TryGetSingletonEntity<Player_Control>(out Entity entityPlayer))
            {
                var playerPosition = SystemAPI.GetComponent<LocalToWorld>(entityPlayer).Position;
                PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
                var world = physicsWorldSingleton.CollisionWorld;
                state.Dependency = new GroundCheckJob
                {
                    hashKey = GetPositionHashMapKey((int3)playerPosition),
                    world = world
                }.ScheduleParallel(state.Dependency);
            }
        }

        [BurstCompile]

        public partial struct GroundCheckJob : IJobEntity
        {
            [ReadOnly] public CollisionWorld world;
            [ReadOnly] public int hashKey;
            void Execute(ref LocalTransform transform, ref CharControllerE control)
            {
                if (control.SkipGroundCheck)
                {
                    return;
                }
                control.IsGrounded = GroundCheck(transform, control, hashKey) ||
                 GroundCheck(transform, control, hashKey + 1) ||
                 GroundCheck(transform, control, hashKey - 1) ||
                 GroundCheck(transform, control, hashKey + quadrantYMultiplier) ||
                 GroundCheck(transform, control, hashKey - quadrantYMultiplier);

            }

            private bool GroundCheck(LocalTransform transform, CharControllerE control, int hashKey)
            {
                if (hashKey == GetPositionHashMapKey((int3)transform.Position))
                {
                    NativeList<RaycastInput> groundRays = new NativeList<RaycastInput>(Allocator.Temp);
                    groundRays.Add(new RaycastInput()
                    {
                        Start = transform.Position + new Unity.Mathematics.float3(0, .2f, 0),
                        End = transform.Position + new Unity.Mathematics.float3(0, -control.GroundCheckDistance, 0),
                        Filter = new CollisionFilter
                        {
                            BelongsTo = ((1 << 10)),
                            CollidesWith = ((1 << 6) | (1 << 9)),
                            GroupIndex = 0
                        }
                    });
                    groundRays.Add(new RaycastInput()
                    {
                        Start = transform.Position + new Unity.Mathematics.float3(0, .2f, .25f),
                        End = transform.Position + new Unity.Mathematics.float3(0, -control.GroundCheckDistance, .25f),
                        Filter = new CollisionFilter
                        {
                            BelongsTo = ((1 << 10)),
                            CollidesWith = ((1 << 6) | (1 << 9)),
                            GroupIndex = 0
                        }
                    });
                    groundRays.Add(new RaycastInput()
                    {
                        Start = transform.Position + new Unity.Mathematics.float3(0, .1f, -.25f),
                        End = transform.Position + new Unity.Mathematics.float3(0, -control.GroundCheckDistance, -.25f),
                        Filter = new CollisionFilter
                        {
                            BelongsTo = ((1 << 10)),
                            CollidesWith = ((1 << 6) | (1 << 9)),
                            GroupIndex = 0
                        }
                    });
                    groundRays.Add(new RaycastInput()
                    {
                        Start = transform.Position + new Unity.Mathematics.float3(.25f, .1f, 0),
                        End = transform.Position + new Unity.Mathematics.float3(.25f, -control.GroundCheckDistance, 0),
                        Filter = new CollisionFilter
                        {
                            BelongsTo = ((1 << 10)),
                            CollidesWith = ((1 << 6) | (1 << 9)),
                            GroupIndex = 0,
                        }
                    });
                    groundRays.Add(new RaycastInput()
                    {
                        Start = transform.Position + new Unity.Mathematics.float3(-.25f, .1f, 0),
                        End = transform.Position + new Unity.Mathematics.float3(-.25f, -control.GroundCheckDistance, 0),
                        Filter = new CollisionFilter
                        {
                            BelongsTo = ((1 << 10)),
                            CollidesWith = ((1 << 6) | (1 << 9)),
                            GroupIndex = 0
                        }
                    });

                    foreach (var ray in groundRays)
                    {

                        NativeList<Unity.Physics.RaycastHit> raycastArray = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);

                        if (world.CastRay(ray, ref raycastArray))
                        {
                            groundRays.Dispose();
                            return true;
                        }
                    }
                    return false;
                }
                else
                    return false;
            }
        }
        [BurstCompile]
        partial struct SetQuadrantDataHashMapJob : IJobEntity
        {
            public NativeParallelMultiHashMap<int, QuadrantData>.ParallelWriter quadrantMap;
            public void Execute(Entity entity, [ReadOnly] in LocalTransform transform)
            {
                int hashMapKey = GetPositionHashMapKey(transform.Position);
                quadrantMap.Add(hashMapKey, new QuadrantData
                {
                    entity = entity,
                    position = transform.Position
                });
            }
        }
    }
}
