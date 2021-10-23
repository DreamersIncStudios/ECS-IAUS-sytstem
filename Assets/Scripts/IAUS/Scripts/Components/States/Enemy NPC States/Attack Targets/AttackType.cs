using Unity.Entities;
using UnityEngine;
using IAUS.ECS.Consideration;
using System;
using AISenses;
using IAUS.ECS.StateBlobSystem;

namespace IAUS.ECS.Component
{
    [Serializable]
    public struct AttackTypeInfo : IBufferElementData
    {
        public AttackStyle style;
        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index;

     public ConsiderationScoringData HealthRatio => stateRef.Value.Array[Index].Health;
         public ConsiderationScoringData RangeToTarget => stateRef.Value.Array[Index].Distance;
       public ConsiderationScoringData ManaAmmoAmount => stateRef.Value.Array[Index].ManaAmmo;

        public Target AttackTarget;
        public int DistanceToTarget => !AttackTarget.Equals(default(Target)) ? (int)AttackTarget.DistanceTo : -1;
        public uint AttackRange;
        public float Attacktimer; //TODO This need to be derive from Character stats Possibly
        public float mod { get { return 1.0f - (1.0f / 3.0f); } } //Todo This need to be set by StateBlob System
        public bool InRangeForAttack => DistanceToTarget < AttackRange;
        public float Score;
    }

    public enum AttackStyle { 
        Melee, Range, MagicRange, MagicMelee
    }
}