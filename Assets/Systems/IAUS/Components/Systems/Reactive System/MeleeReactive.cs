using Components.MovementSystem;
using IAUS.ECS.Component;
using IAUS.ECS.Component.Aspects;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using AISenses.VisionSystems.Combat;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Utilities.ReactiveSystem;
using Random = Unity.Mathematics.Random;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive {
    public partial struct MeleeTagReactor : IComponentReactorTagsForAIStates<MeleeAttackTag, MeleeAttackSubState>
    {
        public void ComponentAdded(Entity entity, ref MeleeAttackTag newComponent,
            ref MeleeAttackSubState AIStateCompoment)
        {
        }

        public void ComponentRemoved(Entity entity, ref MeleeAttackSubState AIStateCompoment,
            in MeleeAttackTag oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref MeleeAttackTag newComponent,
            ref MeleeAttackSubState AIStateCompoment, in MeleeAttackTag oldComponent)
        {

        }

        partial class ReactiveSystem : AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, MeleeTagReactor>
        {
            protected override MeleeTagReactor CreateComponentReactor()
            {
                return new MeleeTagReactor();
            }

        }

        partial class MeleeReactiveSystem : SystemBase
        {
            private EntityQuery meleeAttackersRemoved;

            private EntityQuery meleeAttackersAdded;

            protected override void OnCreate()
            {
                base.OnCreate();
                meleeAttackersAdded = GetEntityQuery(new EntityQueryDesc()
                {
                    All = new[]
                    {
                        ComponentType.ReadWrite(typeof(MeleeAttackSubState)),
                        ComponentType.ReadWrite(typeof(AttackState)), ComponentType.ReadWrite(typeof(MeleeAttackTag)),
                        ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(LocalTransform)),
                        ComponentType.ReadOnly(typeof(AttackTarget))
                    },
                    Absent = new[]
                    {
                        ComponentType.ReadOnly(
                            typeof(AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, MeleeTagReactor>.
                                StateComponent))
                    }

                });
                meleeAttackersRemoved = GetEntityQuery(new EntityQueryDesc()
                {
                    All = new[]
                    {
                        ComponentType.ReadWrite(typeof(MeleeAttackSubState)),
                        ComponentType.ReadWrite(typeof(AttackState)),
                        ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(LocalTransform)),
                        ComponentType.ReadOnly(
                            typeof(AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, MeleeTagReactor>.
                                StateComponent))
                    },
                    Absent = new[] { ComponentType.ReadWrite(typeof(MeleeAttackTag)) }

                });
            }

            protected override void OnUpdate()
            {
                new SetAttack() { Seed = (uint)UnityEngine.Random.Range(1, 10000) }.ScheduleParallel();
                new MoveToAttackPosition().ScheduleParallel(meleeAttackersAdded);
                new AttackTargetEnemy()
                {
                    Seed = (uint)UnityEngine.Random.Range(1, 10000),
                    DeltaTime = SystemAPI.Time.DeltaTime
                }.ScheduleParallel();
            }

            [BurstCompile]
            private partial struct SetAttack : IJobEntity
            {
                public uint Seed;

                void Execute(ref MeleeAttackTag tag, ref  MeleeAttackSubState state)
                {
                    tag.AttackIndex = state.SelectAttackIndex(Seed);
                }
            }

            [BurstCompile]
            private partial struct MoveToAttackPosition : IJobEntity
            {
                void Execute(ref Movement move, ref AttackTarget target)
                {
                    move.SetLocation(target.AttackTargetLocation);
                }
            }

            private partial struct AttackTargetEnemy : IJobEntity
            {
                public float DeltaTime;
                public uint Seed;
                void Execute(ref Movement move, ref MeleeAttackSubState state, ref MeleeAttackTag tag)
                {
                    if (move.Completed && !state.AttackNow)
                    {
                        state.AttackDelay -= DeltaTime;
                    }else if (state.AttackNow)
                    {
                        Debug.Log($"hit Enemy with attack {tag.AttackIndex}");
                        tag.AttackIndex = state.SelectAttackIndex(Seed);
                    }
                }
            }

        }

    }

    public partial struct MagicMeleeTagReactor : IComponentReactorTagsForAIStates<MagicMeleeAttackTag, AttackState>
    {
        public void ComponentAdded(Entity entity, ref MagicMeleeAttackTag newComponent, ref AttackState AIStateCompoment)
        {

        }

        public void ComponentRemoved(Entity entity, ref AttackState AIStateCompoment, in MagicMeleeAttackTag oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref MagicMeleeAttackTag newComponent, ref AttackState AIStateCompoment, in MagicMeleeAttackTag oldComponent)
        {

        }
    }
    public partial struct MagicTagReactor : IComponentReactorTagsForAIStates<MagicAttackTag, AttackState>
    {
        public void ComponentAdded(Entity entity, ref MagicAttackTag newComponent, ref AttackState AIStateCompoment)
        {

        }

        public void ComponentRemoved(Entity entity, ref AttackState AIStateCompoment, in MagicAttackTag oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref MagicAttackTag newComponent, ref AttackState AIStateCompoment, in MagicAttackTag oldComponent)
        {

        }
    }
    public partial struct RangeTagReactor : IComponentReactorTagsForAIStates<RangeAttackTag, AttackState>
    {
        public void ComponentAdded(Entity entity, ref RangeAttackTag newComponent, ref AttackState AIStateCompoment)
        {

        }

        public void ComponentRemoved(Entity entity, ref AttackState AIStateCompoment, in RangeAttackTag oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref RangeAttackTag newComponent, ref AttackState AIStateCompoment, in RangeAttackTag oldComponent)
        {

        }
    }

}