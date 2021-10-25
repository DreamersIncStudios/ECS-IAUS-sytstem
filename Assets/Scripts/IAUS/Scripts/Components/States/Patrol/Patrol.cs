using UnityEngine;
using Unity.Entities;
using System;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
namespace IAUS.ECS.Component
{
    [Serializable]
    [GenerateAuthoringComponent]
    public struct Patrol : IBaseStateScorer
    {

        public int NumberOfWayPoints;
        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index;
       

        public ConsiderationScoringData DistanceToPoint { get { return stateRef.Value.Array[Index].Health; } }
        public ConsiderationScoringData HealthRatio { get { return stateRef.Value.Array[Index].Distance; } }
        public bool  Complete => BufferZone > distanceToPoint;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; }}
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        public float distanceToPoint, StartingDistance, BufferZone ;

        public float DistanceRatio => (float)distanceToPoint / (float)StartingDistance != Mathf.Infinity ?  Mathf.Clamp01((float)distanceToPoint / (float)StartingDistance ): 0;
        public Waypoint CurWaypoint;
        //public int ThreatTheshold;
        //public float ThreatRatio;
        public int WaypointIndex { get; set; }
        public float mod { get { return 1.0f - (1.0f / 2.0f); } }
        [HideInInspector] public bool UpdatePatrolPoints;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;
    }
    [Serializable]
    public struct PatrolBuilderData {
        public float BufferZone;
        public float CoolDownTime;
    }
    public struct PatrolActionTag : IComponentData {
        public bool UpdateWayPoint;
    
    }
}