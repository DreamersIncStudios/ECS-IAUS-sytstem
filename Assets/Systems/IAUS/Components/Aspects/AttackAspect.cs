using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;

namespace IAUS.ECS.Component.Aspects
{
    public readonly partial struct AttackAspect : IAspect
    {
        readonly TransformAspect Transform;
        readonly RefRW<AttackState> state;

        public float MeleeScore { get { 
                if(state.ValueRO.CapableOfMelee)
                    return 1;
                else return 0;
            } 
        }
        public float MagicMeleeScore { get
            {
                if (state.ValueRO.CapableOfMelee && state.ValueRO.CapableOfMagic)
                    return 1;
                else return 0;
            } } 

        public float MagicScore { get
            {
                if (state.ValueRO.CapableOfMagic)
                    return 1;
                else return 0;
            } }
        public float ProjectileScore { get
            {
                if (state.ValueRO.CapableOfProjectile)
                    return 1;
                else return 0;
            } }


        public float Score { get { return 0; } }
    }
}