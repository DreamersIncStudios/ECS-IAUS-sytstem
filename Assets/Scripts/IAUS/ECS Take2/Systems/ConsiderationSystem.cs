using Unity.Collections;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Jobs;
using IAUS.Core;
using Stats;

namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateConsideration))]
  
    public class ConsiderationSystem : JobComponentSystem
    {
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            JobHandle jobHandle = Entities.ForEach((ref HealthConsideration Health, in PlayerStatComponent stats) =>
            {
                Health.Ratio = Mathf.Clamp01((float)stats.CurHealth / (float)stats.MaxHealth) ;

            }).Schedule(inputDeps);
            
            JobHandle jobHandle2 = Entities.ForEach((ref DistanceToConsideration distanceTo, in LocalToWorld toWorld,  in Patrol patrol, in DynamicBuffer<PatrolBuffer> buffer) =>
            {
                
                float distanceRemaining = Vector3.Distance(buffer[patrol.index].WayPoint.Point, toWorld.Position);
                // make .7f a variable 
                if (distanceRemaining < patrol.BufferZone)
                    distanceRemaining = 0.0f;

                distanceTo.Ratio = distanceRemaining / patrol.DistanceAtStart > .1f ? distanceRemaining / patrol.DistanceAtStart : 0.0f;

            }).Schedule(jobHandle);

            JobHandle jobHandle3 = Entities.ForEach((ref TimerConsideration Timer,in WaitTime wait) =>
            {
                Timer.Ratio = wait.Timer/wait.TimeToWait;

            }).Schedule(jobHandle2);

            JobHandle jobHandle4 = Entities.ForEach((ref DistanceToConsideration distanceTo, in LocalToWorld toWorld, in FollowCharacter follow) =>
            {

                float distanceRemaining = Vector3.Distance(follow.TargetLocation, toWorld.Position);
                // make .7f a variable 
                if (distanceRemaining < .5f)
                    distanceRemaining = 0.0f;
                distanceTo.Ratio = distanceRemaining / follow.DistanceAtStart;

            }).Schedule(jobHandle3);

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
                .Schedule(jobHandle4);
            JobHandle LeaderCheck = Entities
                .ForEach((ref LeaderConsideration Check, in Party party) => 
                {

                    if (party.Leader == Entity.Null)
                    { Check.score = 1; }
                    else
                        Check.score = 0;

                })
                .Schedule(DetectionEnemy);


            return LeaderCheck; ;

        }
    }
}
