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
        public float DistanceToTarget => !AttackTarget.Equals(default(Target)) ? AttackTarget.DistanceTo : -1.0f;
        public uint AttackRange;
        public float Attacktimer; //TODO This need to be derive from Character stats Possibly
        public float mod { get { return 1.0f - (1.0f / 3.0f); } } //Todo This need to be set by StateBlob System
        [BurstDiscard]
        public bool InRangeForAttack => DistanceToTarget < AttackRange;
        public float Score;
    }

    public enum AttackStyle { 
        Melee, Range, MagicRange, MagicMelee
    }
}