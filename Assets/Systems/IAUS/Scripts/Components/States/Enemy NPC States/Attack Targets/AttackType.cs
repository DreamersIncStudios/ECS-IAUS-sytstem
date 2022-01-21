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
        public int Index;

        public ConsiderationScoringData HealthRatio { get { return stateRef.Value.Array[Index].Health; } }
        public ConsiderationScoringData RangeToTarget { get { return stateRef.Value.Array[Index].Distance; } }
        public ConsiderationScoringData ManaAmmoAmount { get { return stateRef.Value.Array[Index].ManaAmmo; } }
        public Target AttackTarget;
        [BurstDiscard]
        public float DistanceToTarget => AttackTarget.entity != Entity.Null? AttackTarget.DistanceTo : -1.0f;
        public uint AttackRange;
        public float AttackDistanceRatio => AttackTarget.entity != Entity.Null ? Mathf.Clamp01( DistanceToTarget / (float)AttackRange) : 1.0f;
        public float Attacktimer; //TODO This need to be derive from Character stats Possibly
        public float mod { get { return 1.0f - (1.0f / 3.0f); } } //Todo This need to be set by StateBlob System
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
                    output = AttackRange >= distToTarget && (AttackRange-7) <= distToTarget; // Make 7 a variable later 
                    break;

            }

            return output;
        }
        public float Score;
    }

    public enum AttackStyle { 
        Melee, Range, MagicRange, MagicMelee
    }
}