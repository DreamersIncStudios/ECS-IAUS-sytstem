using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;



namespace IAUS.ECS2
{
    [RequireComponentTag(typeof(PatrolBuffer))]

    //PatrolAction requires WaitAction just set wait to lowValue;
    [GenerateAuthoringComponent]
    public struct Patrol : BaseStateScorer
    {
        public ConsiderationData Health;
        public ConsiderationData DistanceToTarget;
        [HideInInspector] public bool UpdatePatrolPoints;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _resetTimer;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;
        public float DistanceAtStart;
        public int index;
        public float mod { get { return 1.0f - (1.0f / 2.0f); } }
        public float BufferZone;
        //public int MaxInfluenceAtPoint;
        public float DistInfluence;
        public float3 waypointRef;
        public int MaxNumWayPoint { get; set; }
        public Entity HomeEntity { get; set; }
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public bool Complete => Status == ActionStatus.Running;
        public bool UpdatePosition;
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float ResetTimer { get { return _resetTimer; } set { _resetTimer = value; } }
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public bool CanPatrol;
    }
    public struct PatrolActionTag : IComponentData
    {
        public bool test;

    }
    public struct PatrolUpdateTag : IComponentData { }
}
