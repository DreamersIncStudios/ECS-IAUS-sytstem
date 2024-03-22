using System.Collections;
using System.Collections.Generic;
using IAUS.ECS.Consideration;
using Unity.Entities;
using UnityEngine;
using IAUS.ECS.StateBlobSystem;
using Unity.Transforms;
using Stats.Entities;

namespace IAUS.ECS.Component
{
    public struct EscapeThreat : IMovementState
    {

        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index { get; private set; }
        public AIStates Name { get { return AIStates.RetreatToLocation; } }


        public ConsiderationScoringData DistanceToPosition { get { return stateRef.Value.Array[Index].Health; } }
        public ConsiderationScoringData HealthRatio { get { return stateRef.Value.Array[Index].DistanceToPlaceOfInterest; } }
        public ConsiderationScoringData ThreatInRange => stateRef.Value.Array[Index].EnemyInfluence;

        [SerializeField] public float DistanceRatio => (float)DistanceToPoint / (float)StartingDistance != Mathf.Infinity ? Mathf.Clamp01((float)DistanceToPoint / (float)StartingDistance) : 0;

        public uint NumberOfWayPoints { get { return 1; }
            set {
            }
        }
        [SerializeField] public float DistanceToPoint { get; set; }

        public bool Complete => BufferZone > DistanceToPoint;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }

        [SerializeField] public Waypoint CurWaypoint { get; set; }
        public int WaypointIndex { get; set; }
        public float StartingDistance { get; set; }
        public float BufferZone { get; set; }
        public bool TravelInOrder { get; set; }

        public float CoolDownTime { get { return _coolDownTime; } }

        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public float mod => throw new System.NotImplementedException();
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;

        public EscapeThreat(float coolDownTime) : this()
        {
            _coolDownTime = coolDownTime;
        }

        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }

        public void SetIndex(int index)
        {
            Index = index;
        }
    }

    public readonly partial struct EscapeAspect : IAspect {
        readonly RefRO<LocalTransform> Transform;
        readonly RefRO<AIStat> statInfo;
        readonly RefRW<EscapeThreat> escape;
        float distanceToPoint
        {
            get
            {
                float dist = new();
                if (escape.ValueRO.Complete)
                {
                    dist = 0.0f;
                }
                dist = Vector3.Distance(escape.ValueRO.CurWaypoint.Position, Transform.ValueRO.Position);
                return dist;
            }
        }

        public float Score
        {
            get
            {
                escape.ValueRW.DistanceToPoint = distanceToPoint;
                float totalScore = escape.ValueRO.DistanceToPosition.Output(escape.ValueRO.DistanceRatio) * escape.ValueRO.HealthRatio.Output(statInfo.ValueRO.HealthRatio); //TODO Add Back Later * escape.ValueRO.TargetInRange.Output(attackRatio); ;
                escape.ValueRW.TotalScore = escape.ValueRO.Status != ActionStatus.CoolDown ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * escape.ValueRO.mod) * totalScore) : 0.0f;

                totalScore = escape.ValueRW.TotalScore;
                return totalScore;
            }
        }
        public ActionStatus Status { get => escape.ValueRO.Status; }

    }

}