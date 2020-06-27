using UnityEngine;
using Unity.Burst;
using IAUS.Core;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using IAUS.ECS2.BackGround.Raycasting;
using InfluenceMap;

namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_Initialization))]
    public partial class SetupIAUS : JobComponentSystem
    {

            EntityCommandBufferSystem entityCommandBufferSystem;
            protected override void OnCreate()
            {
                base.OnCreate();
                entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            EntityCommandBuffer.Concurrent entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
            ComponentDataFromEntity<HealthConsideration> health = GetComponentDataFromEntity<HealthConsideration>();
            ComponentDataFromEntity<DistanceToConsideration> Distance = GetComponentDataFromEntity<DistanceToConsideration>();
            ComponentDataFromEntity<TimerConsideration> TimeC = GetComponentDataFromEntity<TimerConsideration>();
            BufferFromEntity<TargetBuffer> targetBuffer = GetBufferFromEntity<TargetBuffer>(true);
            ComponentDataFromEntity<DetectionConsideration> DetectConsider = GetComponentDataFromEntity<DetectionConsideration>();

            ComponentDataFromEntity<Detection> Detect = GetComponentDataFromEntity<Detection>();
            ComponentDataFromEntity<InfluenceValues> Influences = GetComponentDataFromEntity<InfluenceValues>();
            ComponentDataFromEntity<HumanRayCastPoints> HumanRayCastPoint = GetComponentDataFromEntity<HumanRayCastPoints>();
            ComponentDataFromEntity<LeaderConsideration> Leaders = GetComponentDataFromEntity<LeaderConsideration>();

            JobHandle patrolAdd = Entities
                .WithNativeDisableParallelForRestriction(health)
                .WithNativeDisableParallelForRestriction(Distance)
                .WithNativeDisableParallelForRestriction(TimeC)
                .WithNativeDisableParallelForRestriction(Influences)
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
                        if (!Influences.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<InfluenceValues>(nativeThreadIndex, entity);
                        }
                    }
                })
                .WithReadOnly(health)
               .WithReadOnly(TimeC)
               .WithReadOnly(Distance)
               .WithReadOnly(Influences)
                .Schedule(inputDeps);

            JobHandle waitAdd = Entities
                .WithNativeDisableParallelForRestriction(health)
                .WithNativeDisableParallelForRestriction(TimeC)
                .WithNativeDisableParallelForRestriction(Distance)
                .WithNativeDisableParallelForRestriction(entityCommandBuffer)
                .ForEach((Entity entity, int nativeThreadIndex, ref DynamicBuffer<StateBuffer> stateBuffer, in WaitTime c1, in CreateAIBufferTag c2) =>
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
                .WithNativeDisableParallelForRestriction(DetectConsider)

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
                        if (!DetectConsider.Exists(entity))
                            entityCommandBuffer.AddComponent<DetectionConsideration>(nativeThreadIndex, entity);

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
               .WithReadOnly(DetectConsider)
                .Schedule(waitAdd);

            //JobHandle InvestigateAreaAddJob =
            //    Entities
            //        .WithNativeDisableParallelForRestriction(health)
            //        .WithNativeDisableParallelForRestriction(Detect)
            //    .WithNativeDisableParallelForRestriction(entityCommandBuffer)
            //    .ForEach((Entity entity, int nativeThreadIndex, ref DynamicBuffer<StateBuffer> stateBuffer, in InvestigateArea c1, in CreateAIBufferTag c2) =>
            //    {
            //        bool add = true;
            //        for (int index = 0; index < stateBuffer.Length; index++)
            //        {
            //            if (stateBuffer[index].StateName == AIStates.InvestigateArea)
            //            { add = false; }
            //        }

            //        if (add)
            //        {
            //            stateBuffer.Add(new StateBuffer()
            //            {
            //                StateName = AIStates.InvestigateArea,
            //                Status = ActionStatus.Idle
            //            });
            //            if (!health.Exists(entity))
            //            {
            //                entityCommandBuffer.AddComponent<HealthConsideration>(nativeThreadIndex, entity);

            //            }
            //            if (!Detect.Exists(entity))
            //            {
            //                entityCommandBuffer.AddComponent<Detection>(nativeThreadIndex, entity);
            //                throw new System.Exception(" this does not have Detection component attached to game object. Please attach detection in editor and set default value in order to use");
            //            }
            // if (!DetectConsider.Exists(entity))
            //    entityCommandBuffer.AddComponent<DetectionConsideration>(nativeThreadIndex, entity);
            //        }
            //    })
            //   .WithReadOnly(health)
            //   .WithReadOnly(Detect)
            //    .Schedule(DetectionAdd);


            //JobHandle RetreatActionAdd =
            //    Entities
            //        .WithNativeDisableParallelForRestriction(health)
            //        .ForEach((Entity entity, int nativeThreadIndex, ref DynamicBuffer<StateBuffer> stateBuffer, in Retreat retreat, in CreateAIBufferTag c2) =>
            //        {
            //            bool add = true;
            //            for (int index = 0; index < stateBuffer.Length; index++)
            //            {
            //                if (stateBuffer[index].StateName == AIStates.Retreat)
            //                { add = false; }

            //            }

            //            if (add)
            //            {
            //                stateBuffer.Add(new StateBuffer()
            //                {
            //                    StateName = AIStates.Retreat,
            //                    Status = ActionStatus.Idle
            //                });
            //                if (!health.Exists(entity))
            //                {
            //                    entityCommandBuffer.AddComponent<HealthConsideration>(nativeThreadIndex, entity);

            //                }
            //            }
            //        })

            //         .WithReadOnly(health)
            //         .Schedule(InvestigateAreaAddJob);

            JobHandle AddLeaderAndRally = Entities
                    .WithNativeDisableParallelForRestriction(health)
                    .WithNativeDisableParallelForRestriction(Leaders)
                    .WithNativeDisableParallelForRestriction(Detect)
                .WithNativeDisableParallelForRestriction(DetectConsider)

                  .WithNativeDisableParallelForRestriction(targetBuffer)
                .WithNativeDisableParallelForRestriction(HumanRayCastPoint)
                .ForEach((Entity entity, int nativeThreadIndex, ref DynamicBuffer<StateBuffer> stateBuffer, ref Party party, in CreateAIBufferTag c2) =>
                {
                    bool addLeader = true;
                    for (int index = 0; index < stateBuffer.Length; index++)
                    {
                        if (stateBuffer[index].StateName == AIStates.GotoLeader)
                        { addLeader = false; }

                    }
                    bool addRally = true;
                    for (int index = 0; index < stateBuffer.Length; index++)
                    {
                        if (stateBuffer[index].StateName == AIStates.GotoLeader)
                        { addRally = false; }

                    }


                    if (addLeader)
                    {
                        stateBuffer.Add(new StateBuffer()
                        {
                            StateName = AIStates.GotoLeader,
                            Status = ActionStatus.Idle
                        });
                    }

                    if (addRally)
                    {
                        stateBuffer.Add(new StateBuffer()
                        {
                            StateName = AIStates.Rally,
                            Status = ActionStatus.Idle
                        });
                    }
                    if (addLeader || addRally)
                    {
                        if (!health.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<HealthConsideration>(nativeThreadIndex, entity);

                        }
                        if (!Leaders.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<LeaderConsideration>(nativeThreadIndex, entity);

                        }
                        if (!DetectConsider.Exists(entity))
                            entityCommandBuffer.AddComponent<DetectionConsideration>(nativeThreadIndex, entity);
                        if (!targetBuffer.Exists(entity))
                        {
                            entityCommandBuffer.AddBuffer<TargetBuffer>(nativeThreadIndex, entity);
                        }

                        if (!HumanRayCastPoint.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<HumanRayCastPoints>(nativeThreadIndex, entity);
                        }

                        if (!Detect.Exists(entity))
                        {
                            entityCommandBuffer.AddComponent<Detection>(nativeThreadIndex, entity);
                            throw new System.Exception(" this does not have Detection component attached to game object. Please attach detection in editor and set default value in order to use");
                        }
                    }
                })
                                     .WithReadOnly(HumanRayCastPoint)
                .WithReadOnly(health)
                .WithReadOnly(targetBuffer)
               .WithReadOnly(Detect)
               .WithReadOnly(DetectConsider)

                     .Schedule(DetectionAdd);

            // This is to be the last job of this system
            JobHandle ClearCreateTag = Entities
                .WithNativeDisableParallelForRestriction(entityCommandBuffer)

                .ForEach((Entity entity, int nativeThreadIndex, in CreateAIBufferTag Tag) =>
                {
                    entityCommandBuffer.RemoveComponent<CreateAIBufferTag>(nativeThreadIndex, entity);

                })
                .Schedule(AddLeaderAndRally);
            return ClearCreateTag;
        }

    }


}
