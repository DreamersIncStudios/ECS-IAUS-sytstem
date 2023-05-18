using MotionSystem.Components;
using MotionSystem.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;


namespace MotionSystem
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(AnimatorUpdate))]
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
            state.EntityManager.CompleteDependencyBeforeRO<PhysicsWorldSingleton>();
            var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            world.UpdateBodyIndexMap();
            state.Dependency = new GroundCheckJob
            {
                world = world
            }.ScheduleParallel(state.Dependency);
        }


        public partial struct GroundCheckJob : IJobEntity
        {
            [ReadOnly] public CollisionWorld world;
            void Execute(ref LocalTransform transform, ref CharControllerE control)
            {
                if (control.SkipGroundCheck)
                {
                    return;
                }
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

                    if (control.IsGrounded = world.CastRay(ray, ref raycastArray))
                    {
                        groundRays.Dispose();
                        break;
                    }
                }


            }
        }
    }
}
