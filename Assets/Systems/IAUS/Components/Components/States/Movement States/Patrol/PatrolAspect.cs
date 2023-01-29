using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public readonly partial struct PatrolAspect : IAspect
    {

       readonly RefRO<AIStat> statInfo;
        readonly RefRW<Patrol> patrol;

        public float Score {
            get { 
                float totalScore = patrol.ValueRO.DistanceToPoint.Output(patrol.ValueRO.distanceToPoint) * patrol.ValueRO.HealthRatio.Output(statInfo.ValueRO.HealthRatio); //TODO Add Back Later * patrol.ValueRO.TargetInRange.Output(attackRatio); ;
                patrol.ValueRW.TotalScore = patrol.ValueRO.Status != ActionStatus.CoolDown && !patrol.ValueRO.AttackTarget ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * patrol.ValueRO.mod) * totalScore) : 0.0f;

                patrol.ValueRW.TotalScore = totalScore;
                return totalScore;
            }
        }
        public ActionStatus Status { get => patrol.ValueRO.Status; }
    }
    public readonly partial struct TraverseAspect : IAspect
    {

        readonly RefRO<AIStat> statInfo;
        readonly RefRW<Traverse> traverse;
        public float Score
        {
            get
            {
                float totalScore = traverse.ValueRO.DistanceToPoint.Output(traverse.ValueRO.distanceToPoint) * traverse.ValueRO.HealthRatio.Output(statInfo.ValueRO.HealthRatio);  ;
                traverse.ValueRW.TotalScore = traverse.ValueRO.Status != ActionStatus.CoolDown  ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * traverse.ValueRO.mod) * totalScore) : 0.0f;

                traverse.ValueRW.TotalScore = totalScore;
                return totalScore;
            }
        }

        public ActionStatus Status { get => traverse.ValueRO.Status; }

    }

}