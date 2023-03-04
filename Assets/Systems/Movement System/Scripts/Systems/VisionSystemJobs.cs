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

    public class VisionTargetingUpdateGroup : ComponentSystemGroup
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
                    All = new ComponentType[] { ComponentType.ReadOnly(typeof(WorldTransform)), ComponentType.ReadWrite(typeof(AITarget)) }

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
                TargetPosition = TargetEntityQuery.ToComponentDataArray<WorldTransform>(Allocator.TempJob),
                TargetEntity = TargetEntityQuery.ToEntityArray(Allocator.TempJob),
            }.ScheduleParallel(state.Dependency);

        }
        [BurstCompile]
        public partial struct VisionRayCastJob : IJobEntity
        {
            [DeallocateOnJobCompletion][ReadOnly] public NativeArray<AITarget> TargetArray;
            [DeallocateOnJobCompletion][ReadOnly] public NativeArray<WorldTransform> TargetPosition;
            [DeallocateOnJobCompletion][ReadOnly] public NativeArray<Entity> TargetEntity;
            [ReadOnly] public CollisionWorld world;
            void Execute(ref DynamicBuffer<ScanPositionBuffer> buffer, ref WorldTransform transform, ref Vision vision, ref PhysicsInfo physicsInfos)
            {
                buffer.Clear();
                if (TargetArray.Length == 0)
                {
                    return;
                }
                for (int j = 0; j < TargetArray.Length; j++)
                {
                    float dist = Vector3.Distance(transform.Position, TargetPosition[j].Position);
                    if (dist < vision.ViewRadius)
                    {
                        Vector3 dirToTarget = ((Vector3)TargetPosition[j].Position - (Vector3)(transform.Position + new float3(0, 1, 0))).normalized;

                        if (Vector3.Angle(transform.Forward(), dirToTarget) < vision.ViewAngle / 2.0f)
                        {
                            //Todo if Object in when X distance add to list without raycast

                            RaycastInput raycastInput = new RaycastInput()
                            {
                                Start = transform.Position + new float3(0, 1, 0) + transform.Forward() * 3f, //Make offset radius to get rid of self intersection
                                End = TargetPosition[j].Position + TargetArray[j].CenterOffset,
                                Filter = new CollisionFilter()
                                {
                                    BelongsTo = ((1 << 10)),
                                    CollidesWith = physicsInfos.CollidesWith.Value,
                                    GroupIndex = 0
                                }
                            };
                            Debug.DrawLine(raycastInput.Start, raycastInput.End, Color.red);
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