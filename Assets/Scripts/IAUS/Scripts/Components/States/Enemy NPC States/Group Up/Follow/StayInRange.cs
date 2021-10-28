using IAUS.ECS.Consideration;
using UnityEngine;
using Unity.Entities;

namespace IAUS.ECS.Component
{
    [GenerateAuthoringComponent]
    public struct StayInRange : IBaseStateScorer
    {
        public int refIndex { get; set; }

        public float MaxRangeToLeader; // will change to an influence score later 
        public float DistanceToLeader;
        public float influenceScoreAtPosition;
        public float DistanceRatio => DistanceToLeader / MaxRangeToLeader;
        public ConsiderationScoringData DistanceToLead;
       
        public ConsiderationScoringData HealthRatio;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public float mod { get { return 1.0f - (1.0f / 2.0f); } }
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;
    }

    public struct StayInRangeActionTag : IComponentData {
        bool test;
    }
}