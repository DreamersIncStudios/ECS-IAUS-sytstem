using Unity.Collections;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using CharacterStats;

namespace IAUS.ECS2
{
    [UpdateBefore(typeof(StateScoreSystem))]
    public class ConsiderationSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle jobHandle = Entities.ForEach((ref HealthConsideration Health, ref Stats stats) =>
            {
                Health.Ratio = Mathf.Clamp01((float)stats.CurHealth / (float)stats.MaxHealth) ;

            }).Schedule(inputDeps);
            
            JobHandle jobHandle2 = Entities.ForEach((ref DistanceToConsideration distanceTo, ref LocalToWorld toWorld,  ref Patrol patrol, ref DynamicBuffer<PatrolBuffer> buffer) =>
            {
                float distanceRemaining = Vector3.Distance(buffer[patrol.index].WayPoint.Point, toWorld.Position);
                // make .7f a variable 
                if (distanceRemaining < .7f)
                    distanceRemaining = 0.0f;
                distanceTo.Ratio =  distanceRemaining/patrol.DistanceAtStart;

            }).Schedule(jobHandle);

            JobHandle jobHandle3 = Entities.ForEach((ref TimerConsideration Timer,ref WaitTime wait) =>
            {
                Timer.Ratio = wait.Timer/wait.TimeToWait;

            }).Schedule(jobHandle2);

            JobHandle DetectionEnemy = new DetectionScore()
            {
                Transforms = GetComponentDataFromEntity<LocalToWorld>(true)
            }.Schedule(this, jobHandle3);

            return DetectionEnemy; ;

        }
    }
}
