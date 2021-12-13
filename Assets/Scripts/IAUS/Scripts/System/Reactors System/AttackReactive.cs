using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using Components.MovementSystem;


[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<AttackActionTag,AttackTargetState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.ManageComponentRemovalJob))]


namespace IAUS.ECS.Systems.Reactive
{
    public struct AttackTagReactor : IComponentReactorTagsForAIStates<AttackActionTag, AttackTargetState>
    {
        public void ComponentAdded(Entity entity, ref AttackActionTag newComponent, ref AttackTargetState AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
            newComponent.StyleOfAttack = AIStateCompoment.HighScoreAttack.style; 
        }

        public void ComponentRemoved(Entity entity, ref AttackTargetState AIStateCompoment, in AttackActionTag oldComponent)
        {
            if ( AIStateCompoment.Status == ActionStatus.Success)
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = 5.0f; // TODO assign in editor 
            }
            else
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = 7.5f ;

            }
        }

        public void ComponentValueChanged(Entity entity, ref AttackActionTag newComponent, ref AttackTargetState AIStateCompoment, in AttackActionTag oldComponent)
        {
        }

        public class AttackReactiveSystem : AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>
        {
            protected override AttackTagReactor CreateComponentReactor()
            {
                return new AttackTagReactor();
            }
        }

    }
    [UpdateAfter(typeof(PatrolMovement))]
    public class AttackSystem : SystemBase
    {
        EntityQuery AttackAdded;
        EntityQuery AttackRemoved;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();

            AttackAdded = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTargetState)), ComponentType.ReadWrite(typeof(AttackActionTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(AttackTypeInfo))
                , ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>.StateComponent)) }
            });
            AttackRemoved = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTargetState)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(AttackTypeInfo))
                , ComponentType.ReadOnly(typeof(LocalToWorld)),ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>.StateComponent))
                },
                None = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackActionTag)) }
            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new MoveToAttackRange() { 
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                AttackChunk = GetComponentTypeHandle<AttackTargetState>(true)
                    
            }.ScheduleParallel(AttackAdded, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

        public struct MoveToAttackRange : IJobChunk
        {
            public ComponentTypeHandle<Movement> MovementChunk;
            [ReadOnly]public ComponentTypeHandle<AttackTargetState> AttackChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Movement> Moves = chunk.GetNativeArray(MovementChunk);
                NativeArray<AttackTargetState> states = chunk.GetNativeArray(AttackChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Movement move = Moves[i];
                    AttackTargetState state = states[i];
                    switch (state.HighScoreAttack.style) {
                        case AttackStyle.Melee:
                        case AttackStyle.MagicMelee:
                            move.TargetLocation = state.HighScoreAttack.AttackTarget.LastKnownPosition;
                        break;

                        case AttackStyle.Range:
                        case AttackStyle.MagicRange:
                            break;

                    }
                    move.SetTargetLocation = move.CanMove= true;
                    Moves[i] = move;
                }

            }
        }

        public struct AttackTarget: IJobChunk
        {
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                throw new System.NotImplementedException();
            }
        }


    }


}