using System.Linq;
using DreamersInc.ComboSystem;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
using Sirenix.Utilities;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public struct AttackState : IBaseStateScorer
    {
        public AttackState(float coolDownTime, bool melee = false, bool magic = false, bool range = false)
        {
            this.coolDownTime = coolDownTime;
            CapableOfMelee = melee;
            CapableOfMagic = magic;
            CapableOfProjectile = range;
            status = ActionStatus.Idle;
            Index = 0;
            resetTime = 0;
            totalScore = 0;
            IsTargeting = true;
        }

        public bool CapableOfMelee;
        public bool CapableOfMagic;
        public bool CapableOfProjectile;
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
    public struct MeleeAttackSubState : IComponentData {
        public int AttackTargetIndex;
        public float3 AttackTargetLocation;
        public Entity TargetEntity{ get; set; }
        public bool TargetInRange;
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public float AttackRange { get; set; } //Todo Pull from character stats speed
        public AIStates Name => AIStates.AttackMelee;
        public float AttackDelay;
       [SerializeField] public bool AttackNow => AttackDelay <= 0.0f;
        public float mod => 1.0f - (1.0f / 3.0f);


    }
    public struct MagicAttackSubState : IComponentData
    {
        public Entity TargetEntity { get; set; }
        public int AttackTargetIndex;
        public float3 AttackTargetLocation;
        public bool TargetInRange;
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public static AIStates Name => AIStates.AttackMagic;
        public float mod => 1.0f - (1.0f / 5.0f);
        public void SetupPossibleAttacks(){}
        
    }
    public struct WeaponSkillsAttackSubState : IComponentData
    {
        public Entity TargetEntity { get; set; }
        public int AttackTargetIndex;
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public static AIStates Name => AIStates.AttackMagicMelee;

        public float Mod => 1.0f - (1.0f / 4.0f);
        public void SetupPossibleAttacks(){}
        
    }
    public struct RangedAttackSubState : IComponentData
    {
        public Entity TargetEntity { get; set; }
        public int AttackTargetIndex;
        public float MaxEffectiveRange;
        public bool TargetInRange;
        public float3 AttackTargetLocation;
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public static AIStates Name => AIStates.AttackRange;
        
       public float Mod => 1.0f - (1.0f / 5.0f);
        public void SetupPossibleAttacks(){}

    }

    public struct MeleeAttackTag : IComponentData
    {
        public int AttackIndex;
        public int PositionIndex;

    }
    public struct MagicAttackTag : IComponentData { }
    public struct RangeAttackTag : IComponentData { }
    public struct WeaponSkillAttackTag : IComponentData { }


    public enum SubAttackStates { }
}