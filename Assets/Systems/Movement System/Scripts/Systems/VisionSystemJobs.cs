using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;
using Global.Component;
using Unity.Mathematics;
using Stats;
using RaycastHit = Unity.Physics.RaycastHit;
using System.Linq;

namespace AISenses.VisionSystems
{

    public partial class VisionTargetingUpdateGroup : ComponentSystemGroup
    {
        public VisionTargetingUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(60, true);
        }

    }
    [UpdateInGroup(typeof(VisionTargetingUpdateGroup))]
    public partial struct VisionSystemJobs : ISystem
    {
        private EntityQuery TargetEntityQuery;

        public void OnCreate(ref SystemState state)
        {
            TargetEntityQuery = state.GetEntityQuery(

                new EntityQueryDesc()
                {
                    All = new ComponentType[] { ComponentType.ReadOnly(typeof(LocalTransform)), ComponentType.ReadWrite(typeof(AITarget)) }

                });
        }

        public void OnDestroy(ref SystemState state)
        {
        }
        public void OnUpdate(ref SystemState state)
        {
            state.EntityManager.CompleteDependencyBeforeRO<PhysicsWorldSingleton>();
            var world = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            world.UpdateBodyIndexMap();
            state.Dependency = new VisionRayCastJob()
            {
                world = world,
                TargetArray = TargetEntityQuery.ToComponentDataArray<AITarget>(Allocator.TempJob),
                TargetPosition = TargetEntityQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob),
                TargetEntity = TargetEntityQuery.ToEntityArray(Allocator.TempJob),
            }.ScheduleParallel(state.Dependency);

        }
        [BurstCompile]
        public partial struct VisionRayCastJob : IJobEntity
        {
            [DeallocateOnJobCompletion][ReadOnly] public NativeArray<AITarget> TargetArray;
            [DeallocateOnJobCompletion][ReadOnly] public NativeArray<LocalTransform> TargetPosition;
            [DeallocateOnJobCompletion][ReadOnly] public NativeArray<Entity> TargetEntity;
            [ReadOnly] public CollisionWorld world;
            void Execute(ref DynamicBuffer<ScanPositionBuffer> buffer, ref LocalTransform transform, ref Vision vision, ref PhysicsInfo physicsInfos)
            {
                buffer.Clear();
                if (TargetArray.Length == 0)
                {
                    return;
                }

                for (int i = 0; i < TargetArray.Length; i++)
                {
                    float dist = Vector3.Distance(transform.Position, TargetPosition[i].Position);
                    if (dist < vision.ViewRadius)
                    {
                        Vector3 dirToTarget = ((Vector3)TargetPosition[i].Position - (Vector3)(transform.Position + new float3(0, 1, 0))).normalized;

                        if (Vector3.Angle(transform.Forward(), dirToTarget) < vision.ViewAngle / 2.0f)
                        {
                            RaycastInput raycastInput = new RaycastInput()
                            {
                                Start = transform.Position + new float3(0, 1, 0) + transform.Forward() * 3f,
                                End = TargetPosition[i].Position + TargetArray[i].CenterOffset,
                                Filter = new CollisionFilter()
                                {
                                    BelongsTo = ((1 << 10)),
                                    CollidesWith = physicsInfos.CollidesWith.Value,
                                    GroupIndex = 0
                                }
                            };
                            if (!world.CastRay(raycastInput, out RaycastHit raycastHit)) continue;
                            if (raycastHit.Entity.Equals(TargetEntity[i]))
                            {

                                buffer.Add(new ScanPositionBuffer()
                                {
                                    target = new Target()
                                    {
                                        CanSee = true,
                                        DistanceTo = dist,
                                        LastKnownPosition = TargetPosition[i].Position,
                                        TargetInfo = TargetArray[i],
                                        Entity = TargetEntity[i]
                                    },
                                    dist = dist

                                });
                            }
                        }
                    }

                }

            }
        }
    }
}