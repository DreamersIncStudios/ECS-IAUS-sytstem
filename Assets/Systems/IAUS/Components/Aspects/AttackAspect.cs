using AISenses.VisionSystems;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;

namespace IAUS.ECS.Component.Aspects
{
    public readonly partial struct AttackAspect : IAspect
    {
        readonly TransformAspect Transform;
        readonly RefRW<AttackState> state;
        readonly RefRW<MeleeAttackSubState> melee;
        readonly RefRW<MagicAttackSubState> magic;
        readonly RefRW<MagicMeleeAttackSubState> MagicMelee;
        readonly RefRW<RangedAttackSubState> Range;
        readonly VisionAspect VisionAspect;
        readonly RefRO<AIStat> statInfo;

        float baseScore
        {
            get
            {
                float temp = new float();
                temp = state.ValueRO.HealthRatio.Output(statInfo.ValueRO.HealthRatio);
                return temp;
            }
        }


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
                {
                    if (VisionAspect.TargetInRange(out float dist))
                    {
                        float totalScore = Range.ValueRO.TargetInRange.Output(dist) *
                                            Range.ValueRO.CoverInRange.Output(0) *
                                            Range.ValueRO.Ammo.Output(statInfo.ValueRO.ManaRatio); //Todo Change this line to be inventory based if we decide to do non mana based projectiles

                        return totalScore;
                    }else
                        return 0;
                }
                else return 0;
            } }


        public float Score { get {
                List < float> scores= new List < float >();
                scores.Add(MeleeScore); scores.Add(MagicMeleeScore);
                scores.Add(MagicScore);
                scores.Add(ProjectileScore);
                return state.ValueRW.TotalScore=scores.Max(); } }


        public ActionStatus Status { get => state.ValueRO.Status; }

    }
}