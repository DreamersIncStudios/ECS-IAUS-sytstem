using UnityEngine;
using Unity.Entities;
using AISenses;

namespace IAUS.ECS2.Component
{

    [GenerateAuthoringComponent]
    public struct MeleeAttackTarget : IBaseStateScorer
    {

        public float Timer;
        public float DistanceToTarget;
       [SerializeField] public bool TimeToAttack => Timer <= 0.0f;

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

    public struct AttackTargetActionTag : IComponentData { bool test; }

}