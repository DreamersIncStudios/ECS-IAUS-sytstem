using UnityEngine;
using Unity.Entities;

namespace IAUS.ECS.Component
{

    [GenerateAuthoringComponent]
    public struct AttackTargetState :IBaseStateScorer
    {
        public float Timer;
        public float HealthRatio;
        public float ManaRatio;
        public float DistanceToTarget;
        public Entity Target;
        public bool InRangeForAttack; // To Be set By Buffer check;
        public float AttackRange;  // To Be set By Buffer check;
        [SerializeField] public bool TimeToAttack => Timer <= 0.0f;


        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public float mod { get { return 1.0f - (1.0f / 3.0f); } }

        public float MeleeScore;
        public float RangedScore;
        public float MagicMeleeScore;
        public float MagicRangedScore;


        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore;
    }

   // public struct AttackTargetActionTag : IComponentData { bool test; }

    public struct MeleeAttackTargetActionTag : IComponentData { bool test; }
    public struct RangedAttackTargetActionTag : IComponentData { bool test; }
    public struct MagicMeleeAttackTargetActionTag : IComponentData { bool test; }
    public struct MagicRangedAttackTargetActionTag : IComponentData { bool test; }



}
