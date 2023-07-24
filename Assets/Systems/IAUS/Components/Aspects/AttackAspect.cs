using AISenses.VisionSystems;
using DreamersInc.InflunceMapSystem;
using Global.Component;
using IAUS.ECS.StateBlobSystem;
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

        BlobAssetReference<AIStateBlobAsset> reference => SetupAIStateBlob.reference;

        float baseScore
        {
            get
            {
                float temp = new float();
                temp = reference.Value.Array[state.ValueRO.Index].Health.Output(statInfo.ValueRO.HealthRatio);
                return temp;
            }
        }
        float TravelInFiveSec { get {
                return 5 * 10; // TODO change to stat dependent statInfo.ValueRO.Speed * 5;   
            }
        }

        public float MeleeScore { get {
                if (state.ValueRO.CapableOfMelee && melee.ValueRO.Index != -1) {
                    if (VisionAspect.TargetInRange(out AITarget target, out float dist))
                    {
                        float range = Mathf.Clamp01(dist /(2*TravelInFiveSec));
                        float influenceDist = Mathf.Clamp01(influenceAspect.DistanceToHighProtection / TravelInFiveSec);
                        float total = reference.Value.Array[melee.ValueRO.Index].DistanceToTarget.Output(range)
                            * reference.Value.Array[melee.ValueRO.Index].EnemyInfluence.Output(0.0f)
                            *baseScore;
                        total = Mathf.Clamp01(total + ((1.0f - total) * melee.ValueRO.mod) * total);
                        return total;
                    }
                    else return 0.0f;
                
                }

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
                        float totalScore = reference.Value.Array[Range.ValueRO.Index].DistanceToTarget.Output(range) *
                                            reference.Value.Array[Range.ValueRO.Index].DistanceToPlaceOfInterest.Output(influenceDist) *
                                            reference.Value.Array[Range.ValueRO.Index].ManaAmmo.Output(statInfo.ValueRO.ManaRatio)*baseScore; //Todo Change this line to be inventory based if we decide to do non mana based projectiles
                        totalScore = Mathf.Clamp01(totalScore + ((1.0f - totalScore) * Range.ValueRO.mod) * totalScore);
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
                scores.Add(MeleeScore); 
                scores.Add(MagicMeleeScore);
                scores.Add(MagicScore);
                scores.Add(ProjectileScore);
                return scores.IndexOf(scores.Max());
            }
        }

        public ActionStatus Status { get => state.ValueRO.Status; }

    }
}