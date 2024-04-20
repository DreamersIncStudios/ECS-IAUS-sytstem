using System;
using System.Linq;
using AISenses.VisionSystems;
using Components.MovementSystem;
using Stats.Entities;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public readonly partial struct AttackAspect : IAspect
    {
        private readonly VisionAspect visionAspect;
        private readonly RefRO<MapVision> mapVision;
        private readonly RefRW<AttackState> state;
        private readonly RefRO<LocalTransform> transform;
        private readonly RefRO<AIStat> stats;
        private readonly RefRW<Movement> move;

        //Todo Move to AIstate to allow for Variablity 
        public bool IsHealthy => stats.ValueRO.HealthRatio > .725f; 
        public bool IsInDanger => stats.ValueRO.HealthRatio < .35f;

        public void DeterminePlan()
        {
            if (state.ValueRO.Plan != AttackPlan.None)
            {
                float dist = new float();
                // Is Plan still valid?
                switch (state.ValueRO.Plan)
                {
                    case AttackPlan.Reset:
                        if (!state.ValueRO.InAttackCooldown)
                        {
                            state.ValueRW.Plan = AttackPlan.None;
                            DeterminePlan();
                        }

                        break;
                    case AttackPlan.Evade:
                        break;
                    case AttackPlan.MoveToLocationMelee:
                        dist = Vector3.Distance(transform.ValueRO.Position, mapVision.ValueRO.Locations.c0);
                        if (dist > 3) return;
                        state.ValueRW.Plan = AttackPlan.AttackMelee;
                        break;
                    case AttackPlan.MoveToLocationMagic:
                        dist = Vector3.Distance(transform.ValueRO.Position, mapVision.ValueRO.Locations.c1);
                        if (dist > 3) return;
                        state.ValueRW.Plan = AttackPlan.AttackMagic;
                        break;
                    case AttackPlan.MoveToLocationRange:
                         dist = Vector3.Distance(transform.ValueRO.Position, mapVision.ValueRO.Locations.c2);
                        if (dist > 3) return;
                        state.ValueRW.Plan = AttackPlan.AttackRange;
                        break;
                    case AttackPlan.AttackMelee:
                        if (state.ValueRO.InAttackCooldown)
                            state.ValueRW.Plan = AttackPlan.Reset;
                        break;
                    case AttackPlan.AttackMagic:
                        if (state.ValueRO.InAttackCooldown)
                            state.ValueRW.Plan = AttackPlan.Reset;
                        break;
                    case AttackPlan.AttackRange:
                        if (state.ValueRO.InAttackCooldown)
                            state.ValueRW.Plan = AttackPlan.Reset;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                // select an attack Plan
                int[] scores = new[]
                {
                    RestScore,
                    TravelToTargetMeleeLocation,
                    TravelToTargetMagicLocation,
                    TravelToTargetRangeLocation,
                    MeleeScore, MagicScore, RangeScore,
                    EvadeTarget

                };
                int maxScore = scores.Max();
                int maxIndex = scores.ToList().IndexOf(maxScore);
                state.ValueRW.Plan = (AttackPlan)(maxIndex+1);
            }
        }


        public void ExecutePlan(float deltaTime)
        {
            switch (state.ValueRO.Plan)
            {
                case AttackPlan.None:
                    Debug.LogError("Npc was able to enter Execute Plan with Plan being established");
                    DeterminePlan();
                    break;
                case AttackPlan.Reset:
                    state.ValueRW.AttackResetTimer -= deltaTime;
                    if (state.ValueRO.AttackResetTimer <= 0.0f)
                        state.ValueRW.AttackResetTimer = 0.0f;
                    break;
                case AttackPlan.MoveToLocationMelee:
                    if(!move.ValueRO.TargetLocation.Equals(mapVision.ValueRO.Locations.c0))
                        move.ValueRW.SetLocation(mapVision.ValueRO.Locations.c0);
                    break;
                case AttackPlan.MoveToLocationMagic:
                    if(!move.ValueRO.TargetLocation.Equals(mapVision.ValueRO.Locations.c1))
                        move.ValueRW.SetLocation(mapVision.ValueRO.Locations.c1);
                    break;
                case AttackPlan.MoveToLocationRange:
                    if(!move.ValueRO.TargetLocation.Equals(mapVision.ValueRO.Locations.c2))
                        move.ValueRW.SetLocation(mapVision.ValueRO.Locations.c2);
                    break;
                case AttackPlan.AttackMelee:
                    Debug.Log("attacking");
                    state.ValueRW.AttackResetTimer = 15; //Todo make a variable based off attack and difficulty 
                    break;
                case AttackPlan.AttackMagic:
                    Debug.Log("attacking");
                    state.ValueRW.AttackResetTimer = 15;
                    break;
                case AttackPlan.AttackRange:
                    Debug.Log("attacking");
                    state.ValueRW.AttackResetTimer = 15;
                    break;
                case AttackPlan.Evade:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        private int MeleeScore
        {
            get
            {
                if (state.ValueRO.InCooldown) return 0;
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
                if (state.ValueRO.InCooldown) return 0;
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
                if (state.ValueRO.InCooldown) return 0;
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
                if (!mapVision.ValueRO.HasMeleeLocation) return 0;
                if (InAttackRange(3)) return 0;
                var temp = 3;
                if (IsInDanger) return temp;
                if (IsHealthy)
                    temp++;
                return temp;

            }
        }

        private int TravelToTargetMagicLocation {
          get
          {
              if (!state.ValueRO.CapableOfMagic||!mapVision.ValueRO.HasMagicLocation||InAttackRange(10)) return 0;
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
                if (!IsInDanger) return 0;
                var temp = 2;
                return temp; 
            }
        }

        bool InAttackRange(float range)
        {
            return visionAspect.TargetEnemyTargetInRange(out _, out float dist) && dist <= range;
        }

    }

    public enum AttackPlan
    {
        None,
        Reset,
        MoveToLocationMelee,
        MoveToLocationMagic,
        MoveToLocationRange,
        AttackMelee,
        AttackMagic,
        AttackRange,
        Evade
    }
}
