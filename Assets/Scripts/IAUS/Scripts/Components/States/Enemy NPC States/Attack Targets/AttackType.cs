using Unity.Entities;
using UnityEngine;

namespace IAUS.ECS2.Component
{
    [System.Serializable]

    public struct AttackTypeInfo : IBufferElementData
    {
        public AttackStyle style;
        public ConsiderationScoringData RangeToTarget;
        public ConsiderationScoringData ManaAmmoAmount;


        public uint DistanceToTarget;
        public uint AttackRange;
        public float Attacktimer; // This need to be derive from Character stats Possible ?
        public Entity Target;
        public bool InRangeForAttack => DistanceToTarget < AttackRange;
    }

    public enum AttackStyle { 
        melee, Range, Magic
    }
}