using UnityEngine;
using Unity.Entities;
using System;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
namespace IAUS.ECS.Component
{
    [Serializable]
    [GenerateAuthoringComponent]
    public struct Patrol :  MovementState
    {

        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index;
        public AIStates name { get { return AIStates.Patrol; } }

        public ConsiderationScoringData DistanceToPoint  => stateRef.Value.Array[Index].Health; 
        public ConsiderationScoringData HealthRatio =>  stateRef.Value.Array[Index].Distance; 
         public ConsiderationScoringData TargetInRange =>   stateRef.Value.Array[Index].TargetInRange; 
        public bool Complete { get { return BufferZone > distanceToPoint; } }
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; }}
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        public int NumberOfWayPoints { get; set; }

        public int WaypointIndex { get; set; }
        public Waypoint CurWaypoint { get; set; }
        public float distanceToPoint { get; set; }
        public float StartingDistance { get; set; }
       [SerializeField] public float BufferZone { get; set; }

        public float DistanceRatio => (float)distanceToPoint / (float)StartingDistance != Mathf.Infinity ?  Mathf.Clamp01((float)distanceToPoint / (float)StartingDistance ): 0;
     

        public float mod { get { return 1.0f - (1.0f / 3.0f); } }
        [HideInInspector] public bool UpdatePatrolPoints;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;
    }

    public interface MovementState: IBaseStateScorer {
        public int NumberOfWayPoints { get; set; }
        public AIStates name { get; }
        public int WaypointIndex { get; set; }
        public Waypoint CurWaypoint { get; set; }
        public float distanceToPoint { get; set; }
        public float StartingDistance { get; set; }
        public float BufferZone { get; set; }
        public ActionStatus Status { get; set; }
        public bool Complete { get; }

    }

    [Serializable]
    public struct PMovementBuilderData {
        public float BufferZone;
        public float CoolDownTime;
    }
    public struct PatrolActionTag : IComponentData {
        public bool UpdateWayPoint;
    
    }
}