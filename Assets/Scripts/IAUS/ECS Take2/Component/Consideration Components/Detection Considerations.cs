using Unity.Collections;
using Unity.Transforms;
using UnityEngine;
using Unity.Entities;
namespace IAUS.ECS2
{
    public struct DetectionConsideration : IComponentData { 
        public float VisibilityRatio;
        public float RangeRatio;

    }
    [Unity.Burst.BurstCompile]
    public struct DetectionScore : IJobForEachWithEntity<DetectionConsideration, Detection>
    {
        [NativeDisableParallelForRestriction] [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Transforms;

        public void Execute(Entity entity, int index, ref DetectionConsideration detectionConsider, ref Detection c1)
        {
            detectionConsider.VisibilityRatio = c1.TargetVisibility;
            if (c1.TargetRef != Entity.Null)
            {
                float dist = Vector3.Distance(Transforms[entity].Position, Transforms[c1.TargetRef].Position);
                detectionConsider.RangeRatio = dist / c1.viewRadius;
            }
            else { detectionConsider.RangeRatio = 1.0f; }
        }
    }


}