using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using IAUS.ECS.Component;


namespace IAUS.ECS2
{
    [UpdateAfter(typeof(ConsiderationSystem))]
    [UpdateBefore(typeof(InfluenceMap.TakeTwo))]
    public class StateScoreSystem : JobComponentSystem
    {
        EntityCommandBufferSystem entityCommandBuffer;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        int interval = 120;
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (UnityEngine.Time.frameCount % interval == 5)
            {
                float DT = Time.DeltaTime;
                var tester = new CheckScores()
                {
                    Patrol = GetComponentDataFromEntity<Patrol>(false),
                    Wait = GetComponentDataFromEntity<WaitTime>(false),
                    Move = GetComponentDataFromEntity<Movement>(false),
                    entityCommandBuffer = entityCommandBuffer.CreateCommandBuffer()
                }.Schedule(this, inputDeps);

                JobHandle PatrolJob = Entities.ForEach((ref Patrol patrol, ref HealthConsideration health, ref DistanceToConsideration distanceTo, ref TimerConsideration timer) =>
                {

                    if (patrol.Status != ActionStatus.Running)
                    {
                        switch (patrol.Status)
                        {
                            case ActionStatus.CoolDown:
                                if (patrol.ResetTime > 0.0f)
                                {
                                    patrol.ResetTime -= DT;
                                }
                                else
                                {
                                    patrol.Status = ActionStatus.Idle;
                                    patrol.ResetTime = 0.0f;
                                }
                                break;
                            case ActionStatus.Failure:
                                patrol.ResetTime = patrol.ResetTimer / 2.0f;
                                patrol.Status = ActionStatus.CoolDown;
                                break;
                            case ActionStatus.Interrupted:
                                patrol.ResetTime = patrol.ResetTimer / 2.0f;
                                patrol.Status = ActionStatus.CoolDown;

                                break;
                            case ActionStatus.Success:
                                patrol.ResetTime = patrol.ResetTimer;
                                patrol.Status = ActionStatus.CoolDown;
                                break;
                        }
                    }
                    //add math.clamp01
                    // make sure all outputs goto zero

                    float mod = 1.0f - (1.0f / 2.0f);
                    float TotalScore = Mathf.Clamp01(patrol.Health.Output(health.Ratio) *
                     patrol.DistanceToTarget.Output(distanceTo.Ratio));
                    patrol.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * mod) * TotalScore);

                }).Schedule(tester);
                // PatrolJob.Complete();
                DT = Time.DeltaTime;

                JobHandle WaitJob = Entities.ForEach((ref WaitTime Wait, ref HealthConsideration health, ref DistanceToConsideration distanceTo, ref TimerConsideration timer) =>
                {
                    if (Wait.Status != ActionStatus.Running)
                    {
                        switch (Wait.Status)
                        {
                            case ActionStatus.CoolDown:
                                if (Wait.ResetTime > 0.0f)
                                {
                                    Wait.ResetTime -= DT;
                                }
                                else
                                {
                                    Wait.Status = ActionStatus.Idle;
                                    Wait.ResetTime = 0.0f;
                                }
                                break;
                            case ActionStatus.Failure:
                                Wait.ResetTime = Wait.ResetTimer / 2.0f;
                                Wait.Status = ActionStatus.CoolDown;

                                break;
                            case ActionStatus.Interrupted:
                                Wait.ResetTime = Wait.ResetTimer / 2.0f;
                                Wait.Status = ActionStatus.CoolDown;

                                break;
                            case ActionStatus.Success:
                                Wait.ResetTime = Wait.ResetTimer;
                                Wait.Status = ActionStatus.CoolDown;

                                break;
                        }
                    }




                    float mod = 1.0f - (1.0f / 2.0f);
                    float TotalScore = Mathf.Clamp01(Wait.Health.Output(health.Ratio) *
                     Wait.WaitTimer.Output(timer.Ratio));
                    Wait.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * mod) * TotalScore);

                }).Schedule(PatrolJob);



                return WaitJob;
            }
            else
                return inputDeps;
        }
    }
}
