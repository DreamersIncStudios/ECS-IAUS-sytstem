
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using IAUS.ECS2.BackGround.Raycasting;
using InfluenceMap.Factions;

namespace IAUS.ECS2
{
    [UpdateBefore(typeof(ConsiderationSystem))]
    public partial class SetupIAUS : JobComponentSystem
    {

        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            EntityCommandBuffer.Concurrent entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
           ComponentDataFromEntity<HealthConsideration> health = GetComponentDataFromEntity<HealthConsideration>();
           ComponentDataFromEntity<DistanceToConsideration> Distance = GetComponentDataFromEntity<DistanceToConsideration>();
           ComponentDataFromEntity<TimerConsideration> TimeC = GetComponentDataFromEntity<TimerConsideration>();
            BufferFromEntity<TargetBuffer> targetBuffer = GetBufferFromEntity<TargetBuffer>(true);
            ComponentDataFromEntity<Detection> Detect = GetComponentDataFromEntity<Detection>();
            ComponentDataFromEntity<HumanRayCastPoints> HumanRayCastPoint = GetComponentDataFromEntity<HumanRayCastPoints>();

            JobHandle patrolAdd = Entities
                .WithNativeDisableParallelForRestriction(health)
                .WithNativeDisableParallelForRestriction(Distance)
                .WithNativeDisableParallelForRestriction(TimeC)
                .WithNativeDisableParallelForRestriction(entityCommandBuffer)

                .ForEach((Entity entity, int nativeThreadIndex, ref DynamicBuffer<StateBuffer> stateBuffer, ref Patrol c1, in CreateAIBufferTag c2, in DynamicBuffer<PatrolBuffer> buffer) =>
                {
                    bool add = true;
                    for (int index = 0; index < stateBuffer.Length; index++)
                    {
                        if (stateBuffer[index].StateName == AIStates.Patrol)
                        { add = false; }
                    }
                    c1.MaxNumWayPoint = buffer.Length;

                    if (add)
                    {
                        stateBuffer.Add(new StateBuffer()
                        {
                            StateName = AIStates.Patrol,
                            Status = ActionStatus.Idle
                        });
                        if (!health.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<HealthConsideration>(nativeThreadIndex,entity);
                        }
                        if (!TimeC.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<TimerConsideration>(nativeThreadIndex, entity);
                        }
                        if (!Distance.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<DistanceToConsideration>(nativeThreadIndex, entity);
                        }
                    }
                })
                .WithReadOnly(health)
               .WithReadOnly(TimeC)
               .WithReadOnly(Distance)
                .Schedule(inputDeps);

            JobHandle waitAdd = Entities
                .WithNativeDisableParallelForRestriction(health)
                .WithNativeDisableParallelForRestriction(TimeC)
                .WithNativeDisableParallelForRestriction(Distance)
                .WithNativeDisableParallelForRestriction(entityCommandBuffer)
                .ForEach((Entity entity, int nativeThreadIndex, ref DynamicBuffer<StateBuffer> stateBuffer, in WaitTime c1, in CreateAIBufferTag c2)=> 
                {
                    bool add = true;
                    for (int index = 0; index < stateBuffer.Length; index++)
                    {
                        if (stateBuffer[index].StateName == AIStates.Wait)
                        { add = false; }
                    }

                    if (add)
                    {
                        stateBuffer.Add(new StateBuffer()
                        {
                            StateName = AIStates.Wait,
                            Status = ActionStatus.Idle
                        });
                        if (!health.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<HealthConsideration>(nativeThreadIndex, entity);

                        }
                        if (!TimeC.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<TimerConsideration>(nativeThreadIndex, entity);

                        }
                        if (!Distance.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<DistanceToConsideration>(nativeThreadIndex, entity);

                        }
                    }
                })
                                .WithReadOnly(health)
               .WithReadOnly(TimeC)
               .WithReadOnly(Distance)
                .Schedule(patrolAdd);

            JobHandle DetectionAdd = Entities
                .WithNativeDisableParallelForRestriction(targetBuffer)
                .WithNativeDisableParallelForRestriction(health)
                .WithNativeDisableParallelForRestriction(HumanRayCastPoint)
                .WithNativeDisableParallelForRestriction(entityCommandBuffer)
                .ForEach((Entity entity, int nativeThreadIndex, ref DynamicBuffer<StateBuffer> stateBuffer, in Detection c1, in CreateAIBufferTag c2) =>
                {
                    bool add = true;
                    for (int index = 0; index < stateBuffer.Length; index++)
                    {
                        if (stateBuffer[index].StateName == AIStates.Attack_Melee)
                        { add = false; }
                    }

                    if (add)
                    {
                        stateBuffer.Add(new StateBuffer()
                        {
                            StateName = AIStates.Attack_Melee,
                            Status = ActionStatus.Idle
                        });
                        if (!targetBuffer.Exists(entity))
                        {
                            entityCommandBuffer.AddBuffer<TargetBuffer>(nativeThreadIndex, entity);
                        }
                        if (!health.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<HealthConsideration>(nativeThreadIndex, entity);

                        }
                        if (!HumanRayCastPoint.Exists(entity))
                        { 
                            entityCommandBuffer.AddComponent<HumanRayCastPoints>(nativeThreadIndex, entity);
                        }
                    }
                })
                .WithReadOnly(HumanRayCastPoint)
                .WithReadOnly(health)
                .WithReadOnly(targetBuffer)
                .Schedule(waitAdd);

            JobHandle InvestigateAreaAddJob =
                Entities
                    .WithNativeDisableParallelForRestriction(health)
                    .WithNativeDisableParallelForRestriction(Detect)
                .WithNativeDisableParallelForRestriction(entityCommandBuffer)
                .ForEach((Entity entity, int nativeThreadIndex, ref DynamicBuffer<StateBuffer> stateBuffer, in InvestigateArea c1, in CreateAIBufferTag c2) =>
                {
                    bool add = true;
                    for (int index = 0; index < stateBuffer.Length; index++)
                    {
                        if (stateBuffer[index].StateName == AIStates.InvestigateArea)
                        { add = false; }
                    }

                    if (add)
                    {
                        stateBuffer.Add(new StateBuffer()
                        {
                            StateName = AIStates.InvestigateArea,
                            Status = ActionStatus.Idle
                        });
                        if (!health.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<HealthConsideration>(nativeThreadIndex, entity);

                        }
                        if (!Detect.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<Detection>(nativeThreadIndex, entity);
                            throw new System.Exception(" this does not have Detection component attached to game object. Please attach detection in editor and set default value in order to use");
                        }
                    }
                })
               .WithReadOnly(health)
               .WithReadOnly(Detect)
                .Schedule(DetectionAdd);


            // This is to be the last job of this system
            JobHandle ClearCreateTag = Entities
                .WithNativeDisableParallelForRestriction(entityCommandBuffer)

                .ForEach((Entity entity, int nativeThreadIndex, in CreateAIBufferTag Tag) =>
                {
                    entityCommandBuffer.RemoveComponent<CreateAIBufferTag>(nativeThreadIndex, entity);

                })
                .Schedule(InvestigateAreaAddJob);
                return ClearCreateTag;
        }

    }


}
