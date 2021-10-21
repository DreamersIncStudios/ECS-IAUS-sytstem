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

        public ConsiderationScoringData HealthRatio;
        public ConsiderationScoringData RangeToTarget;
        public ConsiderationScoringData ManaAmmoAmount;
        public bool Range;
        public bool ManaAmmo;
        public Target AttackTarget;
        public int DistanceToTarget => !AttackTarget.Equals(default(Target)) ? (int)AttackTarget.DistanceTo : -1;
        public uint AttackRange;
        public float Attacktimer; //TODO This need to be derive from Character stats Possibly

        public bool InRangeForAttack => DistanceToTarget < AttackRange;
        public float Score;
    }

    public enum AttackStyle { 
        Melee, Range, MagicRange, MagicMelee
    }
}