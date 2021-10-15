using UnityEngine;
using Unity.Entities;
using System;
using IAUS.ECS.Consideration;
namespace IAUS.ECS.Component
{
    [Serializable]
    [GenerateAuthoringComponent]
    public struct Patrol : IBaseStateScorer
    {

        //TODO Change BlobRef to AIState
        public int NumberOfWayPoints;
        public BlobAssetReference<ConsiderationBlobAsset> health;
        public BlobAssetReference<ConsiderationBlobAsset> distance;
        public int HrefIndex;
        public int DrefIndex;

        public ConsiderationScoringData DistanceToPoint { get { return health.Value.Array[DrefIndex].Data; } }
        public ConsiderationScoringData HealthRatio { get { return distance.Value.Array[HrefIndex].Data; } }
        public bool  Complete => BufferZone > distanceToPoint;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; }}
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        public float distanceToPoint, StartingDistance, BufferZone ;

        public float DistanceRatio => (float)distanceToPoint / (float)StartingDistance;
        public Waypoint CurWaypoint;
        //public int ThreatTheshold;
        //public float ThreatRatio;
        public int WaypointIndex { get; set; }
      //  public bool TargetingOrigin => CurWaypoint.point.Position.Equals(new Unity.Mathematics.float3());
        public float mod { get { return 1.0f - (1.0f / 2.0f); } }
        [HideInInspector] public bool UpdatePatrolPoints;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;
    }

    public struct PatrolActionTag : IComponentData {
        public bool UpdateWayPoint;
    
    }
}