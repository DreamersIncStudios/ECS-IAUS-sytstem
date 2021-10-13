using UnityEngine;
using Unity.Entities;
using AISenses;
using IAUS.ECS.Consideration;
namespace IAUS.ECS.Component
{
    [GenerateAuthoringComponent]
    public struct MoveToTarget : IBaseStateScorer {
        public int refIndex { get; set; }


        // Need to adjust state to position character based on attackStyle


        public float MaxRangeToLeader; // will change to an influence score later 
        public float DistanceToLeader;
        public bool InRange;
        public ScanPositionBuffer Target;
        public bool HasTarget => Target.target.entity != Entity.Null;

        public float CheckTimer;
        public bool CheckForTarget => CheckTimer <= 0.0f;
        public float DistanceRatio => DistanceToLeader / MaxRangeToLeader;

        public float UpdateTimer; // check if target is still visable and in range


        public ConsiderationScoringData DistanceToLead;

        public ConsiderationScoringData HealthRatio;
        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public float mod { get { return 1.0f - (1.0f / 3.0f); } }
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;
        
    }
    public struct MoveToTargetActionTag : IComponentData { bool Test; }

}
