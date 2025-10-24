using System;
using System.Linq;
using Components.MovementSystem;
using IAUS.ECS.Component;
using Stats.Entities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
namespace IAUS.ECS.Systems
{
    partial struct DetermineAction : IJobEntity
    {
        public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;

        // Thresholds/constants extracted for readability and reuse
        private const float LowHealthThreshold = 0.35f;
        private const float SafeHealthThreshold = 0.425f;
        private const float HighHealthThreshold = 0.65f;
        private const float MeleeRange = 3f;
        private const float TravelMeleeRange = 6f;
        private const float TravelMagicRange = 10f;
        private const float TravelRangeRange = 10f;

        void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, TerrorizeAreaTag state, in AIStat stat, in LocalToWorld transform)
        {
            if (state.AttackPlans.Length != 0) return;

            state.AttackType = DeterminePrimaryAttackType(state, stat, transform);

            // Build scored plan list as (plan, score) pairs to keep index without re-searching
            var scoredPlans = new (AttackPlan plan, int score)[]
            {
                (AttackPlan.None, -1),
                (AttackPlan.Rest, ComputeRestScore(state, stat)),
                (AttackPlan.Wander, WanderScore),
                (AttackPlan.GetTargetLocation, ScoreGetTargetLocation(state)),
                (AttackPlan.GetAttackLocation, ScoreGetAttackLocation(state)),
                (AttackPlan.MoveToLocationMelee, ScoreTravelToTargetMelee(state, stat, transform)),
                (AttackPlan.MoveToLocationMagic, ScoreTravelToTargetMagic(state, stat, transform)),
                (AttackPlan.MoveToLocationRange, ScoreTravelToTargetRange(state, stat, transform)),
                (AttackPlan.AttackMelee, ScoreMelee(state, stat, transform)),
                (AttackPlan.AttackMagic, ScoreMagic(state, stat, transform)),
                (AttackPlan.AttackRange, ScoreRange(state, stat, transform)),
                (AttackPlan.Evade, ScoreEvade(state, stat))
            };
            foreach (var entry in scoredPlans)
            {
                if (entry.score <= 0) return;
                if (state.AttackPlans.Length >= 8) return;
                state.AttackPlans.Add(entry.plan);
                if (state.AttackPlans.Length >= 8) break;
            }
        }

        private HowToAttack DeterminePrimaryAttackType(TerrorizeAreaTag state, AIStat stat, LocalToWorld transform)
        {
            // Score attack types and pick the best
            var typeScores = new (HowToAttack type, int score)[]
            {
                (HowToAttack.None, -1),
                (HowToAttack.Melee, ScoreMelee(state, stat, transform)),
                (HowToAttack.Magic, ScoreMagic(state, stat, transform)),
                (HowToAttack.Range, ScoreRange(state, stat, transform))
            };
            System.Array.Sort(typeScores, (a, b) => b.score.CompareTo(a.score));
            return typeScores[0].type;
        }
          private int ScoreMelee(TerrorizeAreaTag state, AIStat stats, LocalToWorld transform)
        {
            if (state.InAttackCooldown || !state.CapableOfMelee) return 0;

            var score = 2;
            if (IsHealthAtLeast(stats, SafeHealthThreshold)) score++;

            if (!IsCoverInRange() && (state.CapableOfMagic || state.CapableOfProjectile))
            {
                // Favor melee a bit more when no cover and other options exist
                score++;
            }

            if (HasMultipleAttackCapabilities(state))
            {
                if (!state.CapableOfMagic && IsManaLow()) score++;
                if (!state.CapableOfProjectile && IsAmmoLow()) score++;
            }

            if (!IsInAttackRange(state, MeleeRange, transform)) return score;
            return score + 1;
        }

        private int ScoreRange(TerrorizeAreaTag state, AIStat stats, LocalToWorld transform)
        {
            if (state.InAttackCooldown || !state.CapableOfProjectile) return 0;

            var score = 2;
            if (!IsCoverInRange()) score++;

            if (HasMultipleAttackCapabilities(state))
            {
                if (!state.CapableOfMagic && IsManaLow()) score++;
                if (!state.CapableOfMelee && !IsAmmoLow()) score++;
            }

            if (!IsInAttackRange(state, MeleeRange, transform)) return score;
            return score + 1;
        }

        private int ScoreMagic(TerrorizeAreaTag state, AIStat stats, LocalToWorld transform)
        {
            if (state.InAttackCooldown || !state.CapableOfMagic) return 0;

            var score = 2;
            if (!IsCoverInRange()) score++;

            if (HasMultipleAttackCapabilities(state))
            {
                if (!state.CapableOfMelee && !IsManaLow()) score++;
                if (!state.CapableOfProjectile && IsAmmoLow()) score++;
            }

            if (!IsInAttackRange(state, MeleeRange, transform)) return score;
            return score + 1;
        }

        private static bool IsHealthAtLeast(AIStat stats, float threshold) => stats.HealthRatio > threshold;

        private bool IsCoverInRange()
        {
            return false;
        }

        private bool IsManaLow()
        {
            return false;
        }

        private bool IsAmmoLow()
        {
            return false;
        }

        private bool IsInAttackRange(in TerrorizeAreaTag state, float range, in LocalToWorld transform)
        {
            if (state.AttackPosition.Equals(float3.zero)) return false;
            var distance = Vector3.Distance(transform.Position, state.AttackPosition);
            return distance <= range;
        }

        private static bool HasMultipleAttackCapabilities(in TerrorizeAreaTag state) =>
            state is { CapableOfMagic: true, CapableOfMelee: true }
                or { CapableOfMagic: true, CapableOfProjectile: true }
                or { CapableOfMelee: true, CapableOfProjectile: true };

        private int ScoreTravelToTargetMelee(TerrorizeAreaTag state, AIStat stats, LocalToWorld transform)
        {
            if (!state.CapableOfMelee) return 0;
            if (IsInAttackRange(state, TravelMeleeRange, transform)) return 0;

            int score = 1;
            if (stats.HealthRatio < LowHealthThreshold) return score;

            score++;
            if (stats.HealthRatio > HighHealthThreshold) score++;
            return score;
        }

        private int ScoreTravelToTargetMagic(TerrorizeAreaTag state, AIStat stats, LocalToWorld transform)
        {
            if (!state.CapableOfMagic || IsInAttackRange(state, TravelMagicRange, transform)) return 0;

            var score = 3;
            if (stats.HealthRatio > LowHealthThreshold)
            {
                if (state.CapableOfMelee) score++;
                else return score;
            }
            if (stats.HealthRatio > HighHealthThreshold) score++;
            return score;
        }

        private int ScoreTravelToTargetRange(TerrorizeAreaTag state, AIStat stats, LocalToWorld transform)
        {
            if (!state.CapableOfProjectile || IsInAttackRange(state, TravelRangeRange, transform)) return 0;

            var score = 2;
            if (!(stats.HealthRatio < LowHealthThreshold)) return score;

            if (state.CapableOfMelee) score++;
            return score;
        }

        private int WanderScore => 0;

        private int ScoreEvade(TerrorizeAreaTag state, AIStat stats)
        {
            return stats.HealthRatio > LowHealthThreshold ? 0 : 2;
        }

        private int ComputeRestScore(TerrorizeAreaTag state, AIStat stats)
        {
            var score = 0;
            if (stats.HealthRatio < LowHealthThreshold) return score;
            if (state.InAttackCooldown) score = 3;
            return score;
        }

        private static int ScoreGetAttackLocation(in TerrorizeAreaTag state) => state.AttackPosition.Equals(float3.zero) ? 10 : 0;

        private static int ScoreGetTargetLocation(in TerrorizeAreaTag state) => state.TargetPosition.Equals(float3.zero) ? 10 : 0;
    }
      public partial struct ExecuteTerrorizeAction : IJobEntity
    {
        public  float DeltaTime;
        public EntityCommandBuffer.ParallelWriter ECB;
        private void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, ref TerrorizeAreaTag state, ref Movement move)
        {
            if (state.AttackPlans.IsEmpty) return;
                        switch (state.AttackPlans[0])
            {
                case AttackPlan.None:
                    Debug.LogError("Npc was able to enter Execute Plan with Plan being established");
                    //     DeterminePlan();
                    break;
                case AttackPlan.Rest:
                    state.AttackResetTimer -= DeltaTime;
                    if (state.AttackResetTimer <= 0.0f)
                    {
                        state.AttackResetTimer = 0.0f;
                        state.AttackPlans.RemoveAt(0);
                    }

                    break;
                case AttackPlan.MoveToLocationMelee:
                case AttackPlan.MoveToLocationMagic:
                case AttackPlan.MoveToLocationRange:
                    if (!move.TargetLocation.Equals(state.TargetPosition) &&
                        !state.AttackPosition.Equals(float3.zero))
                        move.SetLocation(state.AttackPosition);
                    if (move.DistanceRemaining < 5)
                        state.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackMelee:
                    state.AttackResetTimer = 15; //Todo make a variable based off attack and difficulty 
                    ECB.AddComponent<SelectAndAttack>(chunkIndex, entity);
                    state.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackMagic:
                    state.AttackResetTimer = 15;
                    state.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackRange:
                    state.AttackResetTimer = 15;
                    state.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.Evade:
                    break;
                case AttackPlan.GetAttackLocation:

                    switch (state.AttackType)
                    {
                        case HowToAttack.None:
                            break;
                        case HowToAttack.Melee:
                            break;
                        case HowToAttack.Magic:
                            break;
                        case HowToAttack.Range:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
    }
}
