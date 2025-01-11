using System.Linq;
using Components.MovementSystem;
using IAUS.Components.Systems;
using ProjectDawn.Navigation;
using Stats.Entities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public readonly partial struct TerrorizeAreaAspect : IAspect
    {
        private readonly RefRW<TerrorizeAreaState> state;
        private readonly RefRO<LocalTransform> transform;
        private readonly RefRO<AIStat> stats;
        private readonly RefRW<Movement> move;
        private readonly RefRO<AgentBody> agent;
        private readonly InteractablesAspect interactablesInRange;
     
        private bool IsHealthy => stats.ValueRO.HealthRatio > .725f;
        private bool IsInDanger => stats.ValueRO.HealthRatio < .35f;
       
        bool InAttackRange(float range)
        {
            if(state.ValueRO.TargetPosition.Equals(float3.zero)) return false;
            var distance = Vector3.Distance(transform.ValueRO.Position, state.ValueRO.TargetPosition);
            return distance <= range;
        }
        
        public void DeterminePlan()
        {
            if(state.ValueRO.AttackPlans.Length!=0)return;
            // select an attack Plan
            int[] scores= new[]
            {
                -1,
                RestScore,
                GetAttackLocation,
                TravelToTargetMeleeLocation,
                TravelToTargetMagicLocation,
                TravelToTargetRangeLocation,
                MeleeScore, MagicScore, RangeScore,
                -1

            };
            var sortedScores = scores.ToList().OrderByDescending(x => x);
            foreach (var score in sortedScores)
            {
                if (score <= 0) continue;
                if(state.ValueRW.AttackPlans.Length>=8)return;
                var index = scores.ToList().IndexOf(score);
                state.ValueRW.AttackPlans.Add((AttackPlan)(index));
            }

        }

        public void ExecutePlan(Entity entity, int chunkIndex, float deltaTime, EntityCommandBuffer.ParallelWriter ECB)
        {
            if(state.ValueRO.AttackPlans.IsEmpty)return;
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
                case AttackPlan.GetAttackLocation:
                    break;
                case AttackPlan.MoveToLocationMelee:
                case AttackPlan.MoveToLocationMagic:
                case AttackPlan.MoveToLocationRange:
                    if(!move.ValueRO.TargetLocation.Equals(state.ValueRO.TargetPosition) && !state.ValueRO.TargetPosition.Equals(float3.zero))
                        move.ValueRW.SetLocation(state.ValueRO.TargetPosition);
                    if(agent.ValueRO.RemainingDistance<5)
                        state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackMelee:
                    Debug.Log("attacking");
                    state.ValueRW.AttackResetTimer = 5; //Todo make a variable based off attack and difficulty 
                    ECB.AddComponent<SelectAndAttack>(chunkIndex, entity);
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackMagic:
                    Debug.Log("attacking");
                    state.ValueRW.AttackResetTimer = 8;
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackRange:
                    Debug.Log("attacking");
                    state.ValueRW.AttackResetTimer = 8;
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
            }
        }

        private int RestScore {
            get
            {
                var temp = 0;
                if (IsInDanger) return temp;
                if(state.ValueRO.InAttackCooldown)
                    temp = 3;
                return temp; 
            }
        }
        private int GetAttackLocation => state.ValueRO.TargetPosition.Equals(float3.zero) ? 10 : 0;
        private int TravelToTargetMeleeLocation {
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

        private int TravelToTargetMagicLocation {
            get
            {
                if (!state.ValueRO.CapableOfMagic||InAttackRange(10)) return 0;
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

        private int TravelToTargetRangeLocation {
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
        private int MeleeScore
        {
            get
            {
                if (state.ValueRO.InAttackCooldown) return 0;
                if (!state.ValueRO.CapableOfMelee) return 0;
                var temp = 2;
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
                var temp = 2;
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
                var temp = 2;
                if (!InAttackRange(30)) return temp;
                temp++;
                return temp;
            }
        }
  
    }
}