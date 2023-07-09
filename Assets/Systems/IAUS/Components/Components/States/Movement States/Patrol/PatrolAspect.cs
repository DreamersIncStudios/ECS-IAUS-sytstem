using DreamersInc.InflunceMapSystem;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
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

        float distanceToPoint
        {
            get
            {
                return patrol.ValueRO.Complete ? 0.0f: Vector3.Distance(patrol.ValueRO.CurWaypoint.Position, transform.ValueRO.Position);
            }
        }
        public float Score
        {
            get
            {
                patrol.ValueRW.distanceToPoint = distanceToPoint;
                float totalScore = patrol.ValueRO.DistanceToPoint.Output(patrol.ValueRO.DistanceRatio) * patrol.ValueRO.HealthRatio.Output(statInfo.ValueRO.HealthRatio); //TODO Add Back Later * escape.ValueRO.TargetInRange.Output(attackRatio); ;
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
        readonly InfluenceAspect influenceAspect;
        readonly RefRO<AIStat> statInfo;
        readonly RefRW<Traverse> traverse;

        float distanceToPoint
        {
            get
            {
                return traverse.ValueRO.Complete ? 0.0f : Vector3.Distance(traverse.ValueRO.CurWaypoint.Position, transform.ValueRO.Position);
            }
        }

        public float Score
        {
            get
            {
                traverse.ValueRW.distanceToPoint = distanceToPoint;
                float totalScore = traverse.ValueRO.DistanceToPoint.Output(traverse.ValueRO.DistanceRatio) * traverse.ValueRO.HealthRatio.Output(statInfo.ValueRO.HealthRatio)
                    * traverse.ValueRO.ThreatInRange.Output(influenceAspect.ThreatRatio);
                traverse.ValueRW.TotalScore = traverse.ValueRO.Status != ActionStatus.CoolDown ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * traverse.ValueRO.mod) * totalScore) : 0.0f;

                traverse.ValueRW.TotalScore = totalScore;
                return totalScore;
            }
        }

        public ActionStatus Status { get => traverse.ValueRO.Status; }

    }

}