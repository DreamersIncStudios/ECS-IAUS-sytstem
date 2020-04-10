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
                //if (patrol.index >= buffer.Length)
                //   return;
                float distanceRemaining = Vector3.Distance(buffer[patrol.index].WayPoint.Point, toWorld.Position);
                // make .7f a variable 
                if (distanceRemaining < patrol.BufferZone)
                    distanceRemaining = 0.0f;
                distanceTo.Ratio =  distanceRemaining/patrol.DistanceAtStart;

            }).Schedule(jobHandle);

            JobHandle jobHandle3 = Entities.ForEach((ref TimerConsideration Timer,ref WaitTime wait) =>
            {
                Timer.Ratio = wait.Timer/wait.TimeToWait;

            }).Schedule(jobHandle2);

            ComponentDataFromEntity<LocalToWorld> Transforms = GetComponentDataFromEntity<LocalToWorld>(true);
            JobHandle DetectionEnemy = Entities
                .WithNativeDisableParallelForRestriction(Transforms)
                .ForEach((Entity entity, ref DetectionConsideration detectionConsider,  in Detection c1) =>
                {
                    detectionConsider.VisibilityRatio = c1.TargetVisibility;
                    if (c1.TargetRef != Entity.Null)
                    {
                        float dist = Vector3.Distance(Transforms[entity].Position, Transforms[c1.TargetRef].Position);
                        detectionConsider.RangeRatio = dist / c1.viewRadius;
                    }
                    else { detectionConsider.RangeRatio = 1.0f; }

                })
                .WithReadOnly(Transforms)
                .Schedule(jobHandle3);

            return DetectionEnemy; ;

        }
    }
}
