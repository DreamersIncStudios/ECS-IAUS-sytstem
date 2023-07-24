using IAUS.ECS.Component;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public struct AttackState : IBaseStateScorer
    {
        public bool CapableOfMelee;
        public bool CapableOfMagic;
        public bool CapableOfProjectile;
        public void SetIndex(int index)
        {
            Index = index;
        }

      [SerializeField]  public int Index { get; private set; }
        public AIStates name { get { return AIStates.Attack; } }

        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }
        public ActionStatus Status { get { return _status; } set { _status = value; } }
        public float CoolDownTime { get { return _coolDownTime; } }
        public bool InCooldown => Status == ActionStatus.CoolDown;
        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }
        public float mod { get { return 1.0f - (1.0f / 4.0f); } }


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
        public AIStates name { get { return AIStates.AttackMelee; } }
        public float mod { get { return 1.0f - (1.0f / 3.0f); } }
      
    }
    public struct MagicAttackSubState : IComponentData
    {
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public AIStates name { get { return AIStates.AttackMagic; } }

     /*   public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].DistanceToTarget;
        public ConsiderationScoringData Mana => stateRef.Value.Array[Index].ManaAmmo;
        public ConsiderationScoringData CoverInRange => stateRef.Value.Array[Index].DistanceToPlaceOfInterest;*/
        public float mod { get { return 1.0f - (1.0f / 5.0f); } }
    }
    public struct MagicMeleeAttackSubState : IComponentData
    {
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public AIStates name { get { return AIStates.AttackMagicMelee; } }

  //      public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].DistanceToTarget;
    //    public ConsiderationScoringData Mana => stateRef.Value.Array[Index].ManaAmmo;
        public float mod { get { return 1.0f - (1.0f / 4.0f); } }
    }
    public struct RangedAttackSubState : IComponentData
    {
        public float MaxEffectiveRange;
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public AIStates name { get { return AIStates.AttackRange; } }

        public float mod { get { return 1.0f - (1.0f / 5.0f); } }

    }
    public struct meleeAttackTag : IComponentData {
        public float AttackDelay;
    }
    public struct magicAttackTag : IComponentData { }
    public struct rangeAttackTag : IComponentData { }
    public struct magicmeleeAttackTag : IComponentData { }


    public enum SubAttackStates { }
}