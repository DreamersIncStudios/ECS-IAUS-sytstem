using MotionSystem.Components;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace MotionSystem
{
    public partial struct GroundCheckSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }


        public void OnDestroy(ref SystemState state)
        {
        
        }

        public void OnUpdate(ref SystemState state)
        {
            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var world = physicsWorldSingleton.CollisionWorld;
            state.Dependency = new GroundCheckJob
            {
                physicsWorld = physicsWorldSingleton,
                world = world
            }.ScheduleParallel(state.Dependency);
        }


        public partial struct GroundCheckJob : IJobEntity {
            [ReadOnly] public PhysicsWorldSingleton physicsWorld;
            [ReadOnly] public CollisionWorld world;
            void Execute(ref WorldTransform transform, ref CharControllerE control) {
                if (control.SkipGroundCheck)
                {
                    return;
                }
                NativeList<RaycastInput> groundRays = new NativeList<RaycastInput>(Allocator.Temp);
                groundRays.Add(new RaycastInput()
                {
                    Start = transform.Position + new Unity.Mathematics.float3(0, .2f, 0),
                    End = transform.Position - new Unity.Mathematics.float3(0, -control.GroundCheckDistance, 0),
                    Filter = new CollisionFilter
                    {
                        BelongsTo = ((1 << 10)),
                        CollidesWith = ((1 << 6) | (1 << 9)),
                        GroupIndex = -1
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
                        GroupIndex = -1
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
                control.ApplyRootMotion = false;

                foreach (var ray in groundRays)
                {

                    NativeList<Unity.Physics.RaycastHit> raycastArray = new NativeList<Unity.Physics.RaycastHit>(Allocator.Temp);

                    if (control.IsGrounded = world.CastRay(ray, ref raycastArray))
                    {
                        control.ApplyRootMotion = true;
                        groundRays.Dispose();
                        break;
                    }
                }
                

            }
        }
    }
}
