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
        private readonly RefRW<AttackActionTag> state;
        private readonly RefRW<Movement> move;
        private readonly RefRO<AgentBody> agent;

        public void ExecutePlan(Entity entity, int chunkIndex, float deltaTime, EntityCommandBuffer.ParallelWriter ECB)
        {
            if (state.ValueRO.AttackPlans.IsEmpty) return;
            switch (state.ValueRO.AttackPlans[0])
            {
                case AttackPlan.None:
                    Debug.LogError("Npc was able to enter Execute Plan with Plan being established");
                    //     DeterminePlan();
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
                    if (!move.ValueRO.TargetLocation.Equals(state.ValueRO.TargetPosition) &&
                        !state.ValueRO.AttackPosition.Equals(float3.zero))
                        move.ValueRW.SetLocation(state.ValueRO.AttackPosition);
                    if (agent.ValueRO.RemainingDistance < 5)
                        state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackMelee:
                    state.ValueRW.AttackResetTimer = 15; //Todo make a variable based off attack and difficulty 
                    ECB.AddComponent<SelectAndAttack>(chunkIndex, entity);
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackMagic:
                    state.ValueRW.AttackResetTimer = 15;
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.AttackRange:
                    state.ValueRW.AttackResetTimer = 15;
                    state.ValueRW.AttackPlans.RemoveAt(0);
                    break;
                case AttackPlan.Evade:
                case AttackPlan.GetAttackLocation:

                    switch (state.ValueRO.AttackType)
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