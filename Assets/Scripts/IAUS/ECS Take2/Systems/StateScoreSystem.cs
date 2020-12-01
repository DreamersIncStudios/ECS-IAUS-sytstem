using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using IAUS.Core;
using Stats;

namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateScore))]

    public class StateScoreSystem : SystemBase
    {
        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }


        protected override void OnUpdate()
        {

            float DT = Time.DeltaTime;

            JobHandle systemDeps = Dependency;
            //Patrol Score
            systemDeps = Entities.ForEach((ref Patrol patrol, in PlayerStatComponent stats, in DistanceToConsideration distanceTo) =>
                {

                    if (patrol.Status == ActionStatus.Disabled)
                        return;

                    float TotalScore = Mathf.Clamp01(patrol.Health.Output(stats.HealthRatio) *
                     patrol.DistanceToTarget.Output(distanceTo.Ratio));
                    patrol.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * patrol.mod) * TotalScore);

                }).ScheduleParallel(systemDeps);

            //Wait Score
               systemDeps = Entities
                    .ForEach((ref WaitTime Wait, in DistanceToConsideration distanceTo, in PlayerStatComponent stats ) =>
                {
                    float TotalScore = Mathf.Clamp01(Wait.Health.Output(stats.HealthRatio) *
                     Wait.WaitTimer.Output(Wait.RatioForScore));
                    Wait.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * Wait.mod) * TotalScore);

                }).ScheduleParallel(systemDeps);

            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;


        }
    }
}
