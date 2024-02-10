using System;
using AISenses.VisionSystems;
using DreamersInc.InflunceMapSystem;
using Global.Component;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IAUS.ECS.StateBlobSystem;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Component.Aspects
{
    

    public readonly partial struct AttackAspect : IAspect
    {
        readonly RefRO<LocalTransform> transform;
        readonly VisionAspect visionAspect;
        readonly InfluenceAspect influenceAspect;
        [Optional] readonly RefRW<AttackState> state;
        [Optional] readonly RefRW<MeleeAttackSubState> melee;
        [Optional] readonly RefRW<MagicAttackSubState> magic;
        [Optional] private readonly RefRW<WeaponSkillsAttackSubState> magicMelee;
        [Optional]readonly RefRW<RangedAttackSubState> Range;
        readonly RefRO<AIStat> statInfo;
        private readonly RefRW<IAUSBrain> brain;


        private StateAsset GetAsset(int index)
        {
            if (brain.ValueRO.State.IsCreated)
                return brain.ValueRO.State.Value.Array[index];
            else 
                throw new ArgumentOutOfRangeException(nameof(brain.ValueRO.State),
                    $"Blob asset not set up yet");
        }

        private float BaseScore => GetAsset(state.ValueRO.Index).Health.Output(statInfo.ValueRO.HealthRatio);

        private float TravelInFiveSec => statInfo.ValueRO.Speed * 5;

        private float MeleeScore { get
            {
                if (!melee.IsValid) return 0.0f;
                if (state.ValueRO.CapableOfMelee && melee.ValueRO.Index != -1) {
                    if (visionAspect.TargetInRange(out _, out float dist))
                    {
                        var asset = GetAsset(melee.ValueRO.Index);
                        var range = Mathf.Clamp01(dist /(2*TravelInFiveSec));
                        var influenceDist = Mathf.Clamp01(influenceAspect.DistanceToHighProtection / TravelInFiveSec); 
                        //Debug.Log($"base score: {BaseScore} RangeScore; {asset.DistanceToTargetEnemy.Output(range)} Threat Score: {asset.EnemyInfluence.Output(0.0f)}");
                        var total = asset.DistanceToTargetEnemy.Output(range) * asset.EnemyInfluence.Output(influenceDist)*BaseScore;
                        total = Mathf.Clamp01(total + ((1.0f - total) * melee.ValueRO.mod) * total);
                        return total;
                    }
                    else return 0.0f;
                
                }

                else return 0;
            } 
        }

        private float MagicMeleeScore { get
            {
                if (!magic.IsValid || !melee.IsValid) return 0.0f;
                    return 1;
         
            } }

        private float MagicScore { get
            {
                if (!magic.IsValid) return 0.0f;
                var asset = GetAsset(magic.ValueRO.Index);
                    return 1;

            } 
        }


        private float ProjectileScore
        {
            get
            {
                if (!Range.IsValid) return 0.0f;

                var asset = GetAsset(Range.ValueRO.Index);

                if (visionAspect.TargetInRange(out _, out var dist))
                {


                    var range = Mathf.Clamp01(dist / Range.ValueRO.MaxEffectiveRange);
                    var influenceDist = Mathf.Clamp01(influenceAspect.DistanceToHighProtection / TravelInFiveSec);
                    var totalScore = asset.DistanceToTargetEnemy.Output(range) *
                                     asset.DistanceToPlaceOfInterest.Output(influenceDist) *
                                     asset.ManaAmmo.Output(statInfo.ValueRO.ManaRatio) *
                                     BaseScore; //Todo Change this line to be inventory based if we decide to do non mana based projectiles
                    totalScore = Mathf.Clamp01(totalScore + ((1.0f - totalScore) * melee.ValueRO.mod) * totalScore);
                    return totalScore;
                }
                else
                    return 0;

            }
        }


        public float Score
        {
            get
            {
                var scores = new List<float>
                {
                    MeleeScore,
                    MagicMeleeScore,
                    MagicScore,
                    ProjectileScore
                };
                return state.ValueRW.TotalScore = scores.Max();
            }
        }

        public int HighState
        {
            get
            {
                var scores = new List<float>
                {
                    MeleeScore,
                    MagicMeleeScore,
                    MagicScore,
                    ProjectileScore
                };
                return scores.IndexOf(scores.Max());
            }
        }

        public ActionStatus Status => state.IsValid ? state.ValueRO.Status : ActionStatus.Disabled;
    }
}