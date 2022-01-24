using Unity.Entities;
using UnityEngine;
using IAUS.ECS.Consideration;
using System;
using AISenses;
using IAUS.ECS.StateBlobSystem;
using Unity.Burst;

namespace IAUS.ECS.Component
{
    [Serializable]
    public struct AttackTypeInfo : IBufferElementData
    {
        public AttackStyle style;
        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index { get; set; }

        public ConsiderationScoringData HealthRatio { get { return stateRef.Value.Array[Index].Health; } }
      [SerializeField]  public ConsiderationScoringData RangeToTarget { get { return stateRef.Value.Array[Index].Distance; } }
      [SerializeField]  public ConsiderationScoringData ManaAmmoAmount { get { return stateRef.Value.Array[Index].ManaAmmo; } }
        public Target AttackTarget;
        [BurstDiscard]
        public float DistanceToTarget => AttackTarget.entity != Entity.Null ? AttackTarget.DistanceTo : -1.0f;
        public uint AttackRange;
        public uint FalloffRange => (style == AttackStyle.Melee || style == AttackStyle.MagicMelee)? 0: fallOffRange;
        [SerializeField] uint fallOffRange;
        [SerializeField]public float AttackDistanceRatio => AttackTarget.entity != Entity.Null ? Mathf.Clamp01( DistanceToTarget / (float)AttackRange) :0.0f;
        public float Attacktimer; //TODO This need to be derive from Character stats Possibly
        public float mod { get { return 1.0f - (1.0f / 3.0f); } } 
        [BurstDiscard]
        public bool InRangeForAttack(float distToTarget) {
            bool output = new bool();
            switch (style) {
                case AttackStyle.Melee:
                case AttackStyle.MagicMelee:
                    output = AttackRange >= distToTarget;
                    break;
                case AttackStyle.Range:
                case AttackStyle.MagicRange:

                    output = AttackRange + FalloffRange>= distToTarget  && (AttackRange-fallOffRange) <= distToTarget; 
                    
                    
                    break;

            }

            return output;
        }
        public float Score { get; set;   }
    }

    public enum AttackStyle { 
        Melee, Range, MagicRange, MagicMelee
    }
}