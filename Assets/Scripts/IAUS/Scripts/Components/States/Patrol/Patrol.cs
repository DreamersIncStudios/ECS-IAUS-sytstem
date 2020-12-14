using UnityEngine;
using Unity.Entities;

namespace IAUS.ECS2.Component
{
    [GenerateAuthoringComponent]
    public struct Patrol : IBaseStateScorer
    {

        public float NumberOfWayPoints;
        public ConsiderationScoringData DistanceToPoint;
        public ConsiderationScoringData HealthRatio;
        public bool Complete => DistanceRatio <= .1f;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; }}
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        public float distanceToPoint;
        public float StartingDistance;
        public float DistanceRatio => (float)distanceToPoint / (float)StartingDistance;
        public Waypoint CurWaypoint;
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