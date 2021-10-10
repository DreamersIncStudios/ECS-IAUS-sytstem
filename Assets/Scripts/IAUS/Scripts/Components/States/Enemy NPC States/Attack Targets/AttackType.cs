using Unity.Entities;
using UnityEngine;
using IAUS.ECS.Consideration;
using System;
namespace IAUS.ECS.Component
{
    [Serializable]
    public struct AttackTypeInfo : IBufferElementData
    {
        public AttackStyle style;
        public ConsiderationScoringData HealthRatio;
        public ConsiderationScoringData RangeToTarget;
        public ConsiderationScoringData ManaAmmoAmount;
        public bool Range;
        public bool ManaAmmo;

        public uint DistanceToTarget;
        public uint AttackRange;
        public float Attacktimer; // This need to be derive from Character stats Possible ?
        public Entity Target;
        public bool InRangeForAttack => DistanceToTarget < AttackRange;
        public float Score;
    }

    public enum AttackStyle { 
        Melee, Range, MagicRange, MagicMelee
    }
}