using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using IAUS.Core;

namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateScore))]

    public class StateScoreSystem : JobComponentSystem
    {
        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {

            float DT = Time.DeltaTime;
                JobHandle PatrolJob = Entities.ForEach((ref Patrol patrol, in HealthConsideration health, in DistanceToConsideration distanceTo, in TimerConsideration timer) =>
                {
                    if (!patrol.CanPatrol) {
                        patrol.Status = ActionStatus.Disabled;
                        patrol.TotalScore = 0.0f;
                        return;
                    }

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
                    if (patrol.Status != ActionStatus.Disabled)
                    {
                        float mod = 1.0f - (1.0f / 2.0f);
                        float TotalScore = Mathf.Clamp01(patrol.Health.Output(health.Ratio) *
                         patrol.DistanceToTarget.Output(distanceTo.Ratio));
                        patrol.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * mod) * TotalScore);
                    }
                    else {
                        patrol.TotalScore = 0.0f;
                    }
                }).Schedule(inputDeps);
                DT = Time.DeltaTime;

                JobHandle WaitJob = Entities
                    .ForEach((ref WaitTime Wait, in DistanceToConsideration distanceTo, in TimerConsideration timer, in HealthConsideration health ) =>
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

            JobHandle FindLeader = Entities.ForEach
            ((ref Party party, in HealthConsideration health, in LeaderConsideration LeaderCon, in DetectionConsideration detectConsider,
            in Detection Detect
            ) =>
            {
                if (party.Status != ActionStatus.Running)
                {
                    switch (party.Status)
                    {
                        case ActionStatus.CoolDown:
                            if (party.ResetTime > 0.0f)
                            {
                                party.ResetTime -= DT;
                            }
                            else
                            {
                                party.Status = ActionStatus.Idle;
                                party.ResetTime = 0.0f;
                            }
                            break;
                        case ActionStatus.Failure:
                            party.ResetTime = party.ResetTimer / 2.0f;
                            party.Status = ActionStatus.CoolDown;

                            break;
                        case ActionStatus.Interrupted:
                            party.ResetTime = party.ResetTimer / 2.0f;
                            party.Status = ActionStatus.CoolDown;

                            break;
                        case ActionStatus.Success:
                            party.ResetTime = party.ResetTimer;
                            party.Status = ActionStatus.CoolDown;
                            
                            break;
                    }
                }
                    float mod = 1.0f - (1.0f / 3.0f);

                // fix later
                    float TotalScore = Mathf.Clamp01(party.Health.Output(health.Ratio) *
                     LeaderCon.score * party.ThreatInArea.Output(detectConsider.ThreatInArea));
                    party.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * mod) * TotalScore);
               



            }).Schedule(WaitJob);

                return FindLeader;
            

 
        }
    }
}
