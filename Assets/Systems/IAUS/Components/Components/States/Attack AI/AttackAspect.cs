using System;
using System.Linq;
using AISenses.VisionSystems;
using Components.MovementSystem;
using ProjectDawn.Navigation;
using Stats.Entities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public readonly partial struct AttackAspect : IAspect
    {
        private readonly RefRW<AttackState> state;
        private readonly RefRO<LocalTransform> transform;
        private readonly RefRO<AIStat> stats;
        private readonly RefRW<Movement> move;
        private readonly RefRO<AgentBody> agent;

        //Todo Move to AI state to allow for Variability 
        private bool IsHealthy => stats.ValueRO.HealthRatio > .725f;
        private bool IsInDanger => stats.ValueRO.HealthRatio < .35f;

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
                EvadeTarget

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
                    state.ValueRW.AttackResetTimer = 15; //Todo make a variable based off attack and difficulty 
                    ECB.AddComponent<SelectAndAttack>(chunkIndex, entity);
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackMagic:
                    Debug.Log("attacking");
                    state.ValueRW.AttackResetTimer = 15;
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackRange:
                    Debug.Log("attacking");
                    state.ValueRW.AttackResetTimer = 15;
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.Evade:
                case AttackPlan.GetAttackLocation:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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

        private int EvadeTarget {
            get
            {
                if (!IsInDanger) return 0;
                var temp = 2;
                return temp; 
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

        public Entity TargetEntity =>state.ValueRO.TargetEntity;
        public float3 TargetPosition {
            get => state.ValueRW.TargetPosition;
            set => state.ValueRW.TargetPosition = value;
        }

        bool InAttackRange(float range)
        {
         if(state.ValueRO.TargetPosition.Equals(float3.zero)) return false;
         var distance = Vector3.Distance(transform.ValueRO.Position, state.ValueRO.TargetPosition);
         return distance <= range;
        }

    }

    public enum AttackPlan
    {
        None,
        Rest,
        GetAttackLocation,
        MoveToLocationMelee,
        MoveToLocationMagic,
        MoveToLocationRange,
        AttackMelee,
        AttackMagic,
        AttackRange,
        Evade
    }
}
