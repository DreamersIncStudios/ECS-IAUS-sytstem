using UnityEngine;
using Unity.Entities;

namespace IAUS.ECS.Component
{

    [GenerateAuthoringComponent]
    public struct AttackTargetState :IBaseStateScorer
    {
       // public int refIndex { get; set; }

        public float Timer;
        [SerializeField] public bool TimeToAttack => Timer <= 0.0f;


        public float TotalScore { get { return _totalScore; }  }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public float mod { get { return 1.0f - (1.0f / 3.0f); } }

        public float MeleeScore;
        public float RangedScore;
        public float MagicMeleeScore;
        public float MagicRangedScore;
        public AttackTypeInfo HighScoreAttack;

        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] float _resetTime;
        [SerializeField] float _totalScore => !HighScoreAttack.Equals(default(AttackTypeInfo)) ? HighScoreAttack.Score : 0.0f;
    }

   // public struct AttackTargetActionTag : IComponentData { bool test; }

    public struct MeleeAttackTargetActionTag : IComponentData { bool test; }
    public struct RangedAttackTargetActionTag : IComponentData { bool test; }
    public struct MagicMeleeAttackTargetActionTag : IComponentData { bool test; }
    public struct MagicRangedAttackTargetActionTag : IComponentData { bool test; }



}
