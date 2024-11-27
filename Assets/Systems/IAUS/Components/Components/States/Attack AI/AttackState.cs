using IAUS.Core.GOAP;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public struct  AttackState : IBaseStateScorer
    {
        public float3 TargetPosition;
        public Entity TargetEntity;

        public AttackState(float coolDownTime, bool melee = false, bool magic = false, bool range = false)
        {
            this.coolDownTime = coolDownTime;
            status = ActionStatus.Idle;
            Index = 0;
            resetTime = 0;
            totalScore = 0;
            IsTargeting = true;
            CapableOfMelee = melee;
            CapableOfMagic = magic;
            CapableOfProjectile = range;
            AttackResetTimer = 0.0f;
            TargetPosition = float3.zero;
            TargetEntity = Entity.Null;
            AttackPlans = new FixedList64Bytes<AttackPlan>();
        }

        public  FixedList32Bytes<AttackPlan> AttackPlans;
        public float AttackResetTimer;
        [SerializeField]  public bool InAttackCooldown => AttackResetTimer != 0.0f;
        public bool CapableOfMelee, CapableOfMagic,CapableOfProjectile;
        public void SetIndex(int index)
        {
            Index = index;
        }

        public int Index { get; private set; }
        public AIStates Name => AIStates.Attack;
        
        public float TotalScore { get => totalScore;
            set { totalScore = value; } }
        public ActionStatus Status { get { return status; } set { status = value; } }
        public float CoolDownTime { get { return coolDownTime; } }
        public bool InCooldown => Status == ActionStatus.CoolDown;
        public float ResetTime { get { return resetTime; } set { resetTime = value; } }
        public float mod { get { return 1.0f - (1.0f / 4.0f); } }

          float coolDownTime;
         float resetTime { get; set; }
         float totalScore { get; set; }
         ActionStatus status;
         public bool IsTargeting;
    }
    public struct AttackActionTag : IComponentData {
        public int SubStateNumber;
    }
    
    public enum SubAttackStates { melee, magic, range, magicMelee, magicRange}
}