using UnityEngine;
using Unity.Entities;
using System;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
namespace IAUS.ECS.Component
{
    [Serializable]
    [GenerateAuthoringComponent]
    public struct Traverse : MovementState
    {

        public int NumberOfWayPoints { get; set; }
        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index;
        public AIStates name { get { return AIStates.Traverse; } }


        public ConsiderationScoringData DistanceToPoint { get { return stateRef.Value.Array[Index].Health; } }
        public ConsiderationScoringData HealthRatio { get { return stateRef.Value.Array[Index].Distance; } }
        public ConsiderationScoringData ThreatInRange;
        public bool Complete => BufferZone > distanceToPoint;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        public float distanceToPoint { get; set; }
        public float StartingDistance { get; set; }
        [SerializeField] public float BufferZone { get; set; }
        public float DistanceRatio => (float)distanceToPoint / (float)StartingDistance != Mathf.Infinity ? Mathf.Clamp01((float)distanceToPoint / (float)StartingDistance) : 0;
        public Waypoint CurWaypoint { get; set; }

        public int WaypointIndex { get; set; }
        public float mod { get { return 1.0f - (1.0f / 2.0f); } }


        [HideInInspector] public bool UpdateTravelPoints;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;
    }



    public struct TraverseActionTag : IComponentData
    {
        public bool UpdateWayPoint;

    }
}