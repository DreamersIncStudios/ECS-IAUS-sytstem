using IAUS.Core.GOAP;
using Stats.Entities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace IAUS.ECS.Component
{
  
    public struct AttackActionTag : IComponentData
    {
        public float3 TargetPosition;
        public float3 AttackPosition;
        public Entity TargetEntity;
        public int TargetPositionID;
        public HowToAttack AttackType;
        public  FixedList32Bytes<AttackPlan> AttackPlans;
        public float AttackResetTimer;
        [SerializeField]  public bool InAttackCooldown => AttackResetTimer != 0.0f;
        

    }
    public struct AttackCapable : IComponentData
    {
        public bool CapableOfMelee, CapableOfMagic, CapableOfProjectile;

        public AttackCapable(bool capableOfMelee, bool capableOfMagic, bool capableOfRange)
        {
            CapableOfMelee = capableOfMelee;
            CapableOfMagic = capableOfMagic;
            CapableOfProjectile = capableOfRange;
        }
    }

    public enum AttackPlan
    {
        None,
        Rest,
        Wander,
        GetTargetLocation,
        GetAttackLocation,
        MoveToLocationMelee,
        MoveToLocationMagic,
        MoveToLocationRange,
        AttackMelee,
        AttackMagic,
        AttackRange,
        Evade
    }

    public enum HowToAttack
    {
        None,
        Melee,
        Magic,
        Range
    }
}