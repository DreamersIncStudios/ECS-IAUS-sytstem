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
using DreamersInc.DamageSystem.Interfaces;

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
        EntityQuery AttackTime;
        EntityQuery ActiveAttack;


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
            AttackTime = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTargetState)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(AttackTypeInfo))
                , ComponentType.ReadOnly(typeof(LocalToWorld)),ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>.StateComponent))
              , ComponentType.ReadWrite(typeof(AttackActionTag)) },
                None = new ComponentType[] {ComponentType.ReadOnly(typeof(MeleeAttackTag)), ComponentType.ReadOnly(typeof(RangeAttackTag)), ComponentType.ReadOnly(typeof(RangeMagicAttackTag)), ComponentType.ReadOnly(typeof(MeleeMagicAttackTag))}
            });
            ActiveAttack = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTargetState)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(AttackTypeInfo))
                , ComponentType.ReadOnly(typeof(LocalToWorld)),ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>.StateComponent))
              , ComponentType.ReadWrite(typeof(AttackActionTag)) },
                Any = new ComponentType[] { ComponentType.ReadOnly(typeof(MeleeAttackTag)), ComponentType.ReadOnly(typeof(RangeAttackTag)), ComponentType.ReadOnly(typeof(RangeMagicAttackTag)), ComponentType.ReadOnly(typeof(MeleeMagicAttackTag)) }
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

            systemDeps = new AttackTimer() {
                AttackChunk = GetComponentTypeHandle<AttackTargetState>(false),
                DT = Time.DeltaTime,
                ECB= _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                EntityChunk = GetEntityTypeHandle()
            }.ScheduleParallel(AttackTime, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);


            Dependency = systemDeps;
        }
        [BurstCompile]
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


        public struct AttackTimer : IJobChunk
        {
             public ComponentTypeHandle<AttackTargetState> AttackChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public float DT;
            public EntityCommandBuffer.ParallelWriter ECB;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<AttackTargetState> states = chunk.GetNativeArray(AttackChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    AttackTargetState state = states[i];

                    if (state.InAttackRange) {
                        state.Timer -= DT;
                    }
                    if (state.Timer <= 0.0f) { 
                        //add attack tag
                        switch(state.HighScoreAttack.style){
                            case AttackStyle.Melee:
                                ECB.AddComponent<MeleeAttackTag>( chunkIndex, entities[i]);
                                break;
                            case AttackStyle.MagicMelee:
                                ECB.AddComponent<MeleeMagicAttackTag>( chunkIndex, entities[i]);
                                break;

                            case AttackStyle.Range:
                                ECB.AddComponent<RangeAttackTag>( chunkIndex, entities[i]);
                                break;
                            case AttackStyle.MagicRange:
                                ECB.AddComponent<RangeMagicAttackTag>( chunkIndex, entities[i]);
                                break;
                        }
                    }
                    states[i] = state;
                }
            }
        }
        [BurstCompile]
        public struct AttackTarget: IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public EntityCommandBuffer.ParallelWriter ECB;
            public ComponentTypeHandle<AttackTargetState> AttackChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<AttackTargetState> states = chunk.GetNativeArray(AttackChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    AttackTargetState state = states[i];
                    AdjustHealth health = new AdjustHealth()
                    {
                        Value = 10
                    };

                    switch (state.HighScoreAttack.style)
                    {

                        //Todo trigger animation in full game

                        case AttackStyle.Melee:
                       
                            ECB.AddComponent(chunkIndex, state.HighScoreAttack.AttackTarget.entity, health);
                            ECB.RemoveComponent<MeleeAttackTag>(chunkIndex, entities[i]);
                            state.Timer = 10.0f;
                            break;
                        case AttackStyle.MagicMelee:
                            ECB.AddComponent(chunkIndex, state.HighScoreAttack.AttackTarget.entity, health);

                            ECB.RemoveComponent<MeleeMagicAttackTag>(chunkIndex, entities[i]);
                            state.Timer = 10.0f;
                            break;

                        case AttackStyle.Range:
                            ECB.AddComponent(chunkIndex, state.HighScoreAttack.AttackTarget.entity, health);
                            ECB.RemoveComponent<RangeAttackTag>(chunkIndex, entities[i]);
                            state.Timer = 10.0f;
                            break;
                        case AttackStyle.MagicRange:
                            ECB.AddComponent(chunkIndex, state.HighScoreAttack.AttackTarget.entity, health);
                            ECB.RemoveComponent<RangeMagicAttackTag>(chunkIndex, entities[i]);
                            state.Timer = 10.0f;
                            break;
                    }

                }


            }
        }


    }


}