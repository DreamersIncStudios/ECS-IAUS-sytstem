using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
using Unity.Mathematics;
using Unity.Entities;
using Stats.Entities;
using Unity.Transforms;
using DreamersInc.QuadrantSystems;

namespace IAUS.ECS.Component
{
    public struct StayInQuadrant : IBaseStateScorer
    {
        public AIStates name { get { return AIStates.RetreatToQuadrant; } }

        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index { get; private set; }
        public ConsiderationScoringData HealthRatio => stateRef.Value.Array[Index].Health;
        public ConsiderationScoringData DistanceToPoint => stateRef.Value.Array[Index].DistanceToPlaceOfInterest;
        public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].DistanceToTarget;

        public float3 SpawnPosition;
        public int Hashkey
        {
            get { return NPCQuadrantSystem.GetPositionHashMapKey((int3)SpawnPosition); }
        }

        public void SetIndex(int index)
        {
            Index = index;
        }
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }

        public ActionStatus Status { get { return _status; } set { _status = value; } }

        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status == ActionStatus.CoolDown;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        
        public float mod { get { return 1.0f - (1.0f / .0f); } }

        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }
        public bool AttackTarget { get; set; }
    }

    public readonly partial struct StayInQuadrantAspect : IAspect {
        readonly RefRO<LocalTransform> transform;
        readonly RefRO<AIStat> statInfo;
        readonly RefRW<StayInQuadrant> stay;


        float distanceToPoint
        {
            get
            {
                return Vector3.Distance(stay.ValueRO.SpawnPosition, transform.ValueRO.Position);
            }
        }
        public float Score
        {
            get
            {
                float totalScore = stay.ValueRO.DistanceToPoint.Output(distanceToPoint) * stay.ValueRO.HealthRatio.Output(statInfo.ValueRO.HealthRatio); //TODO Add Back Later * escape.ValueRO.TargetInRange.Output(attackRatio); ;
                stay.ValueRW.TotalScore = stay.ValueRO.Status != ActionStatus.CoolDown && !stay.ValueRO.AttackTarget ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * stay.ValueRO.mod) * totalScore) : 0.0f;

                totalScore = stay.ValueRW.TotalScore;
                return totalScore;
            }
        }
        public ActionStatus Status { get => stay.ValueRO.Status; }
    }
}
