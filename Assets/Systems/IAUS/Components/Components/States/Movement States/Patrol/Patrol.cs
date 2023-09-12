using UnityEngine;
using Unity.Entities;
using System;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
using Unity.Mathematics;
namespace IAUS.ECS.Component
{
    [Serializable]
    public struct Patrol : IMovementState
    {

        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index { get; private set; }

        public void SetIndex(int index) {
            Index= index;
        }
        public bool StayInQuadrant;
        public AIStates Name { get { return AIStates.Patrol; } }

        public ConsiderationScoringData HealthRatio => stateRef.Value.Array[Index].Health;
        /// <summary>
        /// Utility score for travel to current waypoint assigned
        /// </summary>
        public ConsiderationScoringData DistanceToPoint => stateRef.Value.Array[Index].DistanceToPlaceOfInterest;

        /// <summary>
        /// Utility score for Attackable target in Ranges
        /// </summary>
        public ConsiderationScoringData TargetEnemyInRange => stateRef.Value.Array[Index].DistanceToTargetEnemy;
        public ConsiderationScoringData Influence => stateRef.Value.Array[Index].EnemyInfluence;


        [SerializeField] public bool Complete { get { return BufferZone > distanceToPoint; } }
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status == ActionStatus.CoolDown;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
       [SerializeField] public uint NumberOfWayPoints { get; set; }
        public bool TravelInOrder { get; set; }

        [SerializeField] public int WaypointIndex { get; set; }
        [SerializeField] public Waypoint CurWaypoint { get; set; }
        [SerializeField]public float distanceToPoint { get; set; }
        [SerializeField] public float StartingDistance { get; set; }
       [SerializeField] public float BufferZone { get; set; }

        public float DistanceRatio => (float)distanceToPoint / (float)StartingDistance != Mathf.Infinity ?  Mathf.Clamp01((float)distanceToPoint / (float)StartingDistance ): 0;
        public bool AttackTarget { get; set; }

        public float mod { get { return 1.0f - (1.0f / 4.0f); } }
        [HideInInspector] public bool UpdatePatrolPoints;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }
    }

    public interface IMovementState: IBaseStateScorer {

        public uint NumberOfWayPoints { get; set; }
        public AIStates Name { get; }
        public int WaypointIndex { get; set; }
        public Waypoint CurWaypoint { get; set; }
        public float distanceToPoint { get; set; }
        public float StartingDistance { get; set; }
        public float BufferZone { get; set; }
        public bool Complete { get; }
        public bool TravelInOrder { get; set; }


    }

    [Serializable]
    public struct MovementBuilderData {
        public float BufferZone;
        public float CoolDownTime;
        public uint Range;
        public uint NumberOfStops;
    }
    public struct PatrolActionTag : IComponentData {
        public bool UpdateWayPoint;
        public float WaitTime;
    
    }
}