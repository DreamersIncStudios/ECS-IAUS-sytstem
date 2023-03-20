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

        public int Index { get; private set; }
        public AIStates name { get { return AIStates.Attack; } }

        public BlobAssetReference<AIStateBlobAsset> stateRef;

        public ConsiderationScoringData Influence => stateRef.Value.Array[Index].EnemyInfluence;
        public ConsiderationScoringData HealthRatio => stateRef.Value.Array[Index].Health;

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

    public struct MeleeAttackSubState : IComponentData {
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }

        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public AIStates name { get { return AIStates.AttackMelee; } }

        public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].DistanceToTarget;
        public float mod { get { return 1.0f - (1.0f / 3.0f); } }

    }
    public struct MagicAttackSubState : IComponentData
    {
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public AIStates name { get { return AIStates.AttackMagic; } }

        public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].DistanceToTarget;
        public ConsiderationScoringData Mana;
        public ConsiderationScoringData CoverInRange;
        public float mod { get { return 1.0f - (1.0f / 5.0f); } }
    }
    public struct MagicMeleeAttackSubState : IComponentData
    {
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public AIStates name { get { return AIStates.AttackMagicMelee; } }

        public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].DistanceToTarget;
        public ConsiderationScoringData Mana;
        public float mod { get { return 1.0f - (1.0f / 4.0f); } }
    }
    public struct RangedAttackSubState : IComponentData
    {
        public int Index { get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public AIStates name { get { return AIStates.AttackRange; } }

        public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].DistanceToTarget;
        public ConsiderationScoringData Ammo => stateRef.Value.Array[Index].ManaAmmo;
        public ConsiderationScoringData CoverInRange => stateRef.Value.Array[Index].DistanceToPlaceOfInterest;
        public float mod { get { return 1.0f - (1.0f / 5.0f); } }

    }
}