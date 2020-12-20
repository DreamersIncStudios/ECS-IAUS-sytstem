
using UnityEngine;
using Unity.Entities;

namespace IAUS.ECS2.Component
{
    public struct StayInRange : IBaseStateScorer
    { 
        public float MaxRangeToLeader;
        public float DistanceToLeader;
        public bool MoveInRange => DistanceToLeader > MaxRangeToLeader; // Possible change to AIState ???
        public ConsiderationScoringData DistanceToLead;
        public ConsiderationScoringData HealthRatio;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public float mod { get { return 1.0f - (1.0f / 2.0f); } }
        [HideInInspector] public bool UpdatePatrolPoints;
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;
    }
}