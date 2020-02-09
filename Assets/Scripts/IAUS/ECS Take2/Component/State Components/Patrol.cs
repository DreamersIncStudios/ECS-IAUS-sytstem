using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
namespace IAUS.ECS2
{
    [GenerateAuthoringComponent]
    public struct Patrol : IComponentData
    {
        public float TimeToWait;
        public float Timer { get; set; }
        public ConsiderationData Health;
        public ConsiderationData DistanceToTarget;
        public ConsiderationData WaitTimer;
        public float TotalScore;

    }
    [BurstCompile]
    public struct ScorePatrolState : IJobForEach<Patrol,HealthConsideration,DistanceToConsideration,TimerConsideration>
    {
        public void Execute(ref Patrol patrol, ref HealthConsideration health, ref DistanceToConsideration distanceTo, ref TimerConsideration timer)
        {
            patrol.TotalScore = patrol.Health.Output(health.Ratio) *
                 patrol.DistanceToTarget.Output(distanceTo.Ratio) *
                 patrol.WaitTimer.Output(timer.Ratio);
        }
    }
}