using System;
using System.Linq;
using Components.MovementSystem;
using IAUS.ECS.Component;
using ProjectDawn.Navigation;
using Stats.Entities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Systems
{
    public readonly partial struct TerrorizeAspect : IAspect
    {
        private readonly RefRW<TerrorizeAreaState> state;
        private readonly RefRO<LocalToWorld> transform;

        private readonly RefRO<AIStat>
            stats; // Ensure the 'AIStat' type/component is implemented and that the corresponding namespace is imported.

        public readonly RefRW<Movement> Move;
        private readonly RefRO<AgentBody> agent;

        public FixedList32Bytes<AttackPlan> Plan => state.ValueRW.AttackPlans;
        public AttackPlan CurAttackStep => state.ValueRO.AttackPlans.IsEmpty ? AttackPlan.None : state.ValueRO.AttackPlans[0];
        public Entity TargetEntity => state.ValueRO.TargetEntity;

        public float3 TargetPosition
        {
            get => state.ValueRW.TargetPosition;
            set => state.ValueRW.TargetPosition = value;
        }

        private bool IsHealthy => stats.ValueRO.HealthRatio > .725f;
        private bool IsInDanger => stats.ValueRO.HealthRatio < .35f;

        private bool MultiAttackStates => (state.ValueRO.CapableOfMagic && state.ValueRO.CapableOfMelee) ||
                                          (state.ValueRO.CapableOfMagic && state.ValueRO.CapableOfProjectile) ||
                                          (state.ValueRO.CapableOfMelee && state.ValueRO.CapableOfProjectile);

        public void DeterminePlan()
        {
            if (state.ValueRO.AttackPlans.Length != 0) return;
            state.ValueRW.AttackType = DetermineHowToAttack();
            // select an attack Plan
            int[] scores = new[]
            {
                -1,
                RestScore,
                Wander,
                GetTargetLocation,
                GetAttackLocation,
                TravelToTargetMeleeLocation,
                TravelToTargetMagicLocation,
                TravelToTargetRangeLocation,
                MeleeScore,
                MagicScore,
                RangeScore,
                EvadeTarget
            };
            var sortedScores = scores.ToList().OrderByDescending(x => x);
            foreach (var score in sortedScores)
            {
                if (score <= 0) continue;
                if (state.ValueRW.AttackPlans.Length >= 8) return;
                var index = scores.ToList().IndexOf(score);
                state.ValueRW.AttackPlans.Add((AttackPlan)(index));
            }
        }

        public HowToAttack DetermineHowToAttack()
        {
            int[] scores = new[]
            {
                -1,
                MeleeScore,
                MagicScore,
                RangeScore,
            };
            var sortedScores = scores.ToList().OrderByDescending(x => x);

            var index = scores.ToList().IndexOf(0);
            return (HowToAttack)(index);
        }

        public void ExecutePlan(Entity entity, int chunkIndex, float deltaTime, EntityCommandBuffer.ParallelWriter ECB)
        {
            if (state.ValueRO.AttackPlans.IsEmpty) return;
            switch (state.ValueRO.AttackPlans[0])
            {
                case AttackPlan.None:
                    Debug.LogError("Npc was able to enter Execute Plan with Plan being established");
                    DeterminePlan();
                    break;
                case AttackPlan.Rest:
                    state.ValueRW.AttackResetTimer -= deltaTime;
                    if (state.ValueRO.AttackResetTimer <= 0.0f)
                    {
                        state.ValueRW.AttackResetTimer = 0.0f;
                        state.ValueRW.AttackPlans.RemoveAt(0);
                    }

                    break;
                case AttackPlan.MoveToLocationMelee:
                case AttackPlan.MoveToLocationMagic:
                case AttackPlan.MoveToLocationRange:
                    if (!Move.ValueRO.TargetLocation.Equals(state.ValueRO.TargetPosition) &&
                        !state.ValueRO.TargetPosition.Equals(float3.zero))
                        Move.ValueRW.SetLocation(state.ValueRO.TargetPosition);
                    if (agent.ValueRO.RemainingDistance < 5)
                        state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackMelee:
                    state.ValueRW.AttackResetTimer = 5; //Todo make a variable based off attack and difficulty 
                    ECB.AddComponent<SelectAndAttack>(chunkIndex, entity);
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackMagic:
                    state.ValueRW.AttackResetTimer = 5;
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackRange:
                    state.ValueRW.AttackResetTimer = 5;
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.Evade:
                case AttackPlan.GetAttackLocation:

                    break;
                case AttackPlan.GetTargetLocation:
                    break;
            }
        }

        bool InAttackRange(float range)
        {
            if (state.ValueRO.AttackPosition.Equals(float3.zero)) return false;
            var distance = Vector3.Distance(transform.ValueRO.Position, state.ValueRO.AttackPosition);
            return distance <= range;
        }

        private bool InSafeHpRange => stats.ValueRO.HealthRatio > .425f;


        bool CoverInRange()
        {
            return false;
        }

        bool ManaLevelLow()
        {
            return false;
        }

        bool AmmoLevelLow()
        {
            return false;
        }

        private int GetAttackLocation => state.ValueRO.AttackPosition.Equals(float3.zero) ? 10 : 0;
        private int GetTargetLocation => state.ValueRO.TargetPosition.Equals(float3.zero) ? 10 : 0;
        private int Wander => state.ValueRO.TargetPosition.Equals(float3.zero) ? 10 : 0;

        private int MeleeScore
        {
            get
            {
                if (state.ValueRO.InAttackCooldown) return 0;
                if (!state.ValueRO.CapableOfMelee) return 0;
                //Todo add Map influence check

                var temp = 2;
                if (InSafeHpRange)
                    temp++;
                if (!CoverInRange() && (state.ValueRO.CapableOfMagic || state.ValueRO.CapableOfProjectile))
                {
                    if (!CoverInRange())
                        temp++;
                }

                if (MultiAttackStates)
                {
                    if (!state.ValueRO.CapableOfMagic && ManaLevelLow())
                        temp++;

                    if (!state.ValueRO.CapableOfProjectile && AmmoLevelLow())
                        temp++;
                }

                if (!InAttackRange(3)) return temp;
                temp++;
                return temp;
            }
        }

        private int MagicScore
        {
            get
            {
                if (state.ValueRO.InAttackCooldown) return 0;
                if (!state.ValueRO.CapableOfMagic) return 0;
                //Todo add Map influence check

                var temp = 2;
                if (!CoverInRange())
                    temp++;
                if (MultiAttackStates)
                {
                    if (!state.ValueRO.CapableOfMelee && !ManaLevelLow())
                        temp++;

                    if (!state.ValueRO.CapableOfProjectile && AmmoLevelLow())
                        temp++;
                }

                if (!InAttackRange(10)) return temp;
                temp++;
                return temp;
            }
        }

        private int RangeScore
        {
            get
            {
                if (state.ValueRO.InAttackCooldown) return 0;
                if (!state.ValueRO.CapableOfProjectile) return 0;
                //Todo add Map influence check
                //Check for cover
                //check for Ammo
                //check for HP
                var temp = 2;
                if (!CoverInRange())
                    temp++;
                if (MultiAttackStates)
                {
                    if (!state.ValueRO.CapableOfMagic && ManaLevelLow())
                        temp++;

                    if (!state.ValueRO.CapableOfMelee && !AmmoLevelLow())
                        temp++;
                }

                if (!InAttackRange(30)) return temp;
                temp++;
                return temp;
            }
        }

        private int TravelToTargetMeleeLocation
        {
            get
            {
                if (!state.ValueRO.CapableOfMelee) return 0;
                if (InAttackRange(6)) return 0;
                var temp = 1;
                if (IsInDanger) return temp;
                temp++;
                if (IsHealthy)
                    temp++;
                return temp;
            }
        }

        private int TravelToTargetMagicLocation
        {
            get
            {
                if (!state.ValueRO.CapableOfMagic || InAttackRange(10)) return 0;
                var temp = 3;
                if (IsInDanger)
                {
                    if (state.ValueRO.CapableOfMelee)
                        temp++;
                    else
                    {
                        return temp;
                    }
                }

                if (IsHealthy)
                    temp++;
                return temp;
            }
        }

        private int TravelToTargetRangeLocation
        {
            get
            {
                if (!state.ValueRO.CapableOfProjectile) return 0;
                var temp = 2;
                if (IsInDanger)
                {
                    if (state.ValueRO.CapableOfMelee)
                        temp++;
                    else
                    {
                        return temp;
                    }
                }

                if (IsHealthy)
                    temp++;
                return temp;
            }
        }

        private int EvadeTarget
        {
            get
            {
                if (!IsInDanger) return 0;
                var temp = 2;
                return temp;
            }
        }

        private int RestScore
        {
            get
            {
                var temp = 0;
                if (IsInDanger) return temp;
                if (state.ValueRO.InAttackCooldown)
                    temp = 3;
                return temp;
            }
        }
    }
}