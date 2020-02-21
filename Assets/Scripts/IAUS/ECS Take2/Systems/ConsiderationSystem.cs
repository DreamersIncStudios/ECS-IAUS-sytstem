using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using CharacterStats;

namespace IAUS.ECS2
{
    [UpdateBefore(typeof(InfluenceMap.TakeTwo))]
    public class ConsiderationSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle jobHandle = Entities.ForEach((ref HealthConsideration Health, ref Stats stats) =>
            {
                Health.Ratio = Mathf.Clamp01((float)stats.CurHealth / (float)stats.MaxHealth) ;

            }).Schedule(inputDeps);
            
            JobHandle jobHandle2 = Entities.ForEach((ref DistanceToConsideration distanceTo, ref LocalToWorld toWorld, ref ECS.Component.Movement move, ref Patrol patrol) =>
            {
                float distanceRemaining = Vector3.Distance(move.TargetLocation, toWorld.Position);
                distanceTo.Ratio =  distanceRemaining/patrol.DistanceAtStart;

            }).Schedule(jobHandle);

            JobHandle jobHandle3 = Entities.ForEach((ref TimerConsideration Timer,ref WaitTime wait) =>
            {
                Timer.Ratio = wait.Timer/wait.TimeToWait;

            }).Schedule(jobHandle2);

            //jobHandle.Complete();
            //jobHandle2.Complete();
            //jobHandle3.Complete();
            return jobHandle3;
        }
    }
}
