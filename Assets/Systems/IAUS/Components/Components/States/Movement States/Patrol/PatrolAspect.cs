using Stats.Entities;
using System;
using AISenses.VisionSystems;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public readonly partial struct PatrolAspect : IAspect
    {
        readonly RefRO<LocalTransform> transform;
        readonly RefRO<AIStat> statInfo;
        readonly RefRW<Patrol> patrol;
        private readonly VisionAspect vision;
        float distanceToPoint => patrol.ValueRO.Complete ? 0.0f : Vector3.Distance(patrol.ValueRO.CurWaypoint.Position, transform.ValueRO.Position);

        public float Score
        {
            get
            {
                if (patrol.ValueRO.Index == -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(patrol), $"Please check Creature list and Consideration Data to make sure {patrol.ValueRO.Name} state is implements");

                }
                if (vision.TargetInReactRange)
                {
                    return 0.0f;
                }
                patrol.ValueRW.distanceToPoint = distanceToPoint;
                float totalScore = patrol.ValueRO.DistanceToPoint.Output(patrol.ValueRO.DistanceRatio) * patrol.ValueRO.HealthRatio.Output(statInfo.ValueRO.HealthRatio); //TODO Add Back Later * wander.ValueRO.TargetInRange.Output(attackRatio); ;
                patrol.ValueRW.TotalScore = patrol.ValueRO.Status != ActionStatus.CoolDown && !patrol.ValueRO.AttackTarget ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * patrol.ValueRO.mod) * totalScore) : 0.0f;

                totalScore = patrol.ValueRW.TotalScore;
                return totalScore;
            }
        }
        public ActionStatus Status { get => patrol.ValueRO.Status; }
    }
    public readonly partial struct TraverseAspect : IAspect
    {
        readonly RefRO<LocalTransform> transform;
        readonly RefRO<AIStat> statInfo;
        readonly RefRW<Traverse> traverse;
        private readonly VisionAspect vision;


        float distanceToPoint => traverse.ValueRO.Complete ? 0.0f : Vector3.Distance(traverse.ValueRO.CurWaypoint.Position, transform.ValueRO.Position);
        public float Score
        {
            get
            {
                traverse.ValueRW.distanceToPoint = distanceToPoint;
                if (vision.TargetInReactRange)
                {
                    return 0.0f;
                }
                
                var totalScore = traverse.ValueRO.DistanceToPoint.Output(traverse.ValueRO.DistanceRatio) * traverse.ValueRO.HealthRatio.Output(statInfo.ValueRO.HealthRatio); 
                traverse.ValueRW.TotalScore = traverse.ValueRO.Status != ActionStatus.CoolDown ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * traverse.ValueRO.mod) * totalScore) : 0.0f;

                traverse.ValueRW.TotalScore = totalScore;
                return totalScore;
            }
        }

        public ActionStatus Status { get => traverse.ValueRO.Status; }

    }

}