using IAUS.ECS.Component;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public struct AttackState : IBaseStateScorer
    {
        public bool CapableOfMelee;
        public bool CapableOfMagic;
        public bool CapableOfProjectile;
        public bool HasAttack { get; set; }
        public void SetIndex(int index)
        {
            Index = index;
        }

      [SerializeField]  public int Index { get; private set; }
        public readonly AIStates name { get { return AIStates.Attack; } }

        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public readonly float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status == ActionStatus.CoolDown;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        public readonly float mod { get { return 1.0f - (1.0f / 4.0f); } }
        public float3 AttackLocation { get; set; }

        [SerializeField] public float _coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }
        [SerializeField] public ActionStatus _status;
  
    }
    public struct AttackActionTag : IComponentData {
        public int SubStateNumber;
    }
    public struct MeleeAttackSubState : IComponentData {
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public float AttackRange { get { return 5.5f; } } //Todo Pull from character stats speed
        public readonly AIStates name { get { return AIStates.AttackMelee; } }
        public readonly float mod { get { return 1.0f - (1.0f / 3.0f); } }
      
    }
    public struct MagicAttackSubState : IComponentData
    {
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public readonly AIStates name { get { return AIStates.AttackMagic; } }

     /*   public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].DistanceToTarget;
        public ConsiderationScoringData Mana => stateRef.Value.Array[Index].ManaAmmo;
        public ConsiderationScoringData CoverInRange => stateRef.Value.Array[Index].DistanceToPlaceOfInterest;*/
        public readonly float mod { get { return 1.0f - (1.0f / 5.0f); } }
    }
    public struct MagicMeleeAttackSubState : IComponentData
    {
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public readonly AIStates name { get { return AIStates.AttackMagicMelee; } }

  //      public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].DistanceToTarget;
    //    public ConsiderationScoringData Mana => stateRef.Value.Array[Index].ManaAmmo;
        public readonly float mod { get { return 1.0f - (1.0f / 4.0f); } }
    }
    public struct RangedAttackSubState : IComponentData
    {
        public float MaxEffectiveRange;
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public readonly AIStates name { get { return AIStates.AttackRange; } }

        public readonly float mod { get { return 1.0f - (1.0f / 5.0f); } }

    }
    public struct MeleeAttackTag : IComponentData {
        public float AttackDelay;
        public float3 AttackLocation;
    }
    public struct MagicAttackTag : IComponentData { }
    public struct RangeAttackTag : IComponentData { }
    public struct MagicMeleeAttackTag : IComponentData { }


    public enum SubAttackStates { }
}