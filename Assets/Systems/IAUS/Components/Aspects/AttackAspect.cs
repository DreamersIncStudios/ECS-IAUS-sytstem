using AISenses.VisionSystems;
using DreamersInc.InflunceMapSystem;
using Global.Component;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Component.Aspects
{
    public readonly partial struct AttackAspect : IAspect
    {
        readonly RefRO<LocalTransform> Transform;
        readonly VisionAspect VisionAspect;
        readonly InfluenceAspect influenceAspect;
        readonly RefRW<AttackState> state;
        readonly RefRW<MeleeAttackSubState> melee;
        readonly RefRW<MagicAttackSubState> magic;
        readonly RefRW<MagicMeleeAttackSubState> MagicMelee;
        readonly RefRW<RangedAttackSubState> Range;
        readonly RefRO<AIStat> statInfo;


        public AITarget Target( float dist) {
            return new AITarget();
        }
        float baseScore
        {
            get
            {
                float temp = new float();
                temp = state.ValueRO.HealthRatio.Output(statInfo.ValueRO.HealthRatio);
                return temp;
            }
        }
        float TravelInFiveSec { get {
                return statInfo.ValueRO.Speed * 5;   
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
                    if (VisionAspect.TargetInRange(out AITarget target,out float dist))
                    {
                      

                        float range = Mathf.Clamp01(dist/Range.ValueRO.MaxEffectiveRange);
                        float influenceDist = Mathf.Clamp01( influenceAspect.DistanceToHighProtection / TravelInFiveSec);
                        float totalScore = Range.ValueRO.TargetInRange.Output(range) *
                                            Range.ValueRO.CoverInRange.Output(influenceDist) *
                                            Range.ValueRO.Ammo.Output(statInfo.ValueRO.ManaRatio); //Todo Change this line to be inventory based if we decide to do non mana based projectiles
                        return totalScore;
                    }else
                        return 0;
                }
                else return 0;
            } }


        public float Score
        {
            get
            {

                List<float> scores = new List<float>();
                scores.Add(MeleeScore);
                scores.Add(MagicMeleeScore);
                scores.Add(MagicScore);
                scores.Add(ProjectileScore);
                return state.ValueRW.TotalScore = scores.Max();
            }
        }

        public int HighState
        {
            get
            {
                List<float> scores = new List<float>();
                scores.Add(MeleeScore); scores.Add(MagicMeleeScore);
                scores.Add(MagicScore);
                scores.Add(ProjectileScore);
                return scores.IndexOf(scores.Max());
            }
        }

        public ActionStatus Status { get => state.ValueRO.Status; }

    }
}