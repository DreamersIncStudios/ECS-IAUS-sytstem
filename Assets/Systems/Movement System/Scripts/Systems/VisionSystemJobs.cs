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
namespace AISenses.VisionSystems
{

    public class VisionTargetingUpdateGroup : ComponentSystemGroup {
        public VisionTargetingUpdateGroup() {
            RateManager = new RateUtils.VariableRateManager(60, true);
        }

    }
    [BurstCompile]
    [UpdateInGroup(typeof(VisionTargetingUpdateGroup))]
    public partial struct VisionSystemJobs : ISystem
    {
        private EntityQuery TargetEntityQuery;

        public void OnCreate(ref SystemState state)
        {
            TargetEntityQuery = state.GetEntityQuery(
                
                new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(WorldTransform)), ComponentType.ReadWrite(typeof(AITarget)) }

            });
        }

        public void OnDestroy(ref SystemState state)
        {
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var world = physicsWorldSingleton.CollisionWorld;
            state.Dependency = new VisionRayCastJob()
            {
                physicsWorld = physicsWorldSingleton,
                world = world,
                TargetArray = TargetEntityQuery.ToComponentDataArray<AITarget>(Allocator.TempJob),
                TargetPosition = TargetEntityQuery.ToComponentDataArray<WorldTransform>(Allocator.TempJob),
                TargetEntity = TargetEntityQuery.ToEntityArray(Allocator.TempJob),
            }.ScheduleParallel(state.Dependency);
            state.Dependency.Complete();

        }
        [BurstCompile]

        public partial struct VisionRayCastJob : IJobEntity {
            [DeallocateOnJobCompletion][ReadOnly] public NativeArray<AITarget> TargetArray;
            [DeallocateOnJobCompletion][ReadOnly] public NativeArray<WorldTransform> TargetPosition;
            [DeallocateOnJobCompletion][ReadOnly] public NativeArray<Entity> TargetEntity;
            [ReadOnly] public PhysicsWorldSingleton physicsWorld;
            [ReadOnly] public CollisionWorld world;
            void Execute(ref DynamicBuffer<ScanPositionBuffer> buffer, ref WorldTransform transform, ref Vision vision, ref PhysicsInfo physicsInfos) {
                buffer.Clear();
                if(TargetArray.Length== 0)
                    return;

                for (int j = 0; j < TargetArray.Length; j++)
                {
                    float dist = Vector3.Distance(transform.Position, TargetPosition[j].Position);
                    if (dist < vision.viewRadius)
                    {
                        Vector3 dirToTarget = ((Vector3)TargetPosition[j].Position - (Vector3)(transform.Position + new float3(0, 1, 0))).normalized;

                        if (Vector3.Angle(transform.Forward(), dirToTarget) < vision.ViewAngle / 2.0f)
                        {
                            RaycastInput raycastInput = new RaycastInput()
                            {
                                Start = transform.Position + new float3(0, 1, 0),
                                End = TargetPosition[j].Position + TargetArray[j].CenterOffset,
                                Filter = new CollisionFilter()
                                {
                                    BelongsTo = physicsInfos.BelongsTo.Value,
                                    CollidesWith = physicsInfos.CollidesWith.Value,
                                    GroupIndex = 0
                                }
                            };
                            if (world.CastRay(raycastInput, out RaycastHit raycastHit))
                            {
                                if (raycastHit.Entity.Equals(TargetEntity[j]))
                                {
                                    buffer.Add(new ScanPositionBuffer()
                                    {
                                        target = new Target()
                                        {
                                            CanSee = true,
                                            DistanceTo = dist,
                                            LastKnownPosition = TargetPosition[j].Position,
                                            TargetInfo = TargetArray[j],
                                            entity = TargetEntity[j]
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

}