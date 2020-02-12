using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using CharacterStats;

namespace IAUS.ECS2
{
    [UpdateAfter(typeof(ConsiderationSystem))]
    [UpdateBefore(typeof(InfluenceMap.TakeTwo))]
    public class StateScoreSystem : JobComponentSystem
    {
       

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float DT = Time.DeltaTime;

            JobHandle PatrolJob = Entities.ForEach((ref Patrol patrol, ref HealthConsideration health, ref DistanceToConsideration distanceTo, ref TimerConsideration timer) =>
            {
                if (patrol.Status != ActionStatus.Running && patrol.ResetTime>0.0f )
                {
                    patrol.ResetTime -= DT;
                }

                float mod = 1.0f - (1.0f / 3.0f);
                float TotalScore = patrol.Health.Output(health.Ratio) *
                 patrol.DistanceToTarget.Output(distanceTo.Ratio) *
                 patrol.WaitTimer.Output(timer.Ratio);
                patrol.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * mod) * TotalScore);

            }).Schedule(inputDeps   );

            JobHandle WaitJob = Entities.ForEach((ref WaitTime Wait, ref HealthConsideration health, ref DistanceToConsideration distanceTo, ref TimerConsideration timer) =>
            {
                if (Wait.Status != ActionStatus.Running && Wait.ResetTime > 0.0f)
                {
                    Wait.ResetTime -= DT;
                }

                float mod = 1.0f - (1.0f / 3.0f);
                float TotalScore = Wait.Health.Output(health.Ratio) *
                Wait.DistanceToTarget.Output(distanceTo.Ratio) *
                 Wait.WaitTimer.Output(timer.Ratio);

                Wait.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * mod) * TotalScore);
          
            }).Schedule(PatrolJob);

            JobHandle UpdateScore = Entities.ForEach((ref TestAI AI, ref WaitTime wait, ref Patrol patrol) =>
            {
                AI.Patrol = patrol.TotalScore;
                AI.wait = wait.TotalScore;


            }).Schedule(WaitJob);
            return WaitJob;
        }
    }

    public struct TestScore : IJobForEachWithEntity_EB<StateBuffer>
    {


        public void Execute(Entity entity, int index, DynamicBuffer<StateBuffer> b0)
        {
            throw new System.NotImplementedException();
        }
    }
}
