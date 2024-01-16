using System;
using Components.MovementSystem;
using IAUS.ECS.Component;
using AISenses.VisionSystems.Combat;
using DreamersInc.CombatSystem;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;
using Utilities.ReactiveSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive {
    [UpdateInGroup(typeof(IAUSUpdateStateGroup))]
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
        [UpdateInGroup(typeof(IAUSUpdateStateGroup))]

        partial class ReactiveSystem : AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, MeleeTagReactor>
        {
            protected override MeleeTagReactor CreateComponentReactor()
            {
                return new MeleeTagReactor();
            }

        }

        [UpdateInGroup(typeof(IAUSUpdateStateGroup))]

        [UpdateAfter(typeof(AttackTagReactor.AttackUpdateSystem))]
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
                var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
                
                new AttackTargetEnemy()
                {
                    Seed = (uint)UnityEngine.Random.Range(1, 10000),
                    DeltaTime = SystemAPI.Time.DeltaTime,
                    Surround = SystemAPI.GetComponentLookup<SurroundCharacter>(),

                    ECB = ecb.CreateCommandBuffer(World.Unmanaged).AsParallelWriter()
                }.ScheduleParallel();

            }


//[BurstCompile]
            private partial struct AttackTargetEnemy : IJobEntity
            {
                public float DeltaTime;
                public uint Seed;
                public EntityCommandBuffer.ParallelWriter ECB;
                [NativeDisableParallelForRestriction] public ComponentLookup<SurroundCharacter> Surround;
                [ReadOnly][NativeDisableParallelForRestriction] public ComponentLookup<LocalTransform> TransformGet;
                void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref Movement move,
                    ref AttackTarget target, ref MeleeAttackSubState state, ref MeleeAttackTag tag,
                    in LocalTransform transform)
                {
                    if (Surround.TryGetComponent(target.TargetEntity, out var surroundCharacter))
                    {
                        var targetInfo = surroundCharacter.PositionsSurroundingCharacter[tag.PositionIndex];
                        var dist = Vector3.Distance(targetInfo.Position, transform.Position);
                        if (dist > 10.5f)
                        {
                            //TODO select a surround position
                            move.SetLocation(targetInfo.Position);
                        }
                        if (move.DistanceRemaining < 4.0f && !state.AttackNow)
                        {
                            state.AttackDelay -= DeltaTime;
                        }         else if (state.AttackNow && move.CanMove)
                        {
                            Debug.Log("We Attack AT DAWN");
                        }
                    }
                    else
                    {
                        var enemyTransform = TransformGet[target.TargetEntity];
                        var dist = Vector3.Distance(enemyTransform.Position, transform.Position);
                        if (dist > 10.5f)
                        {
                            //TODO select a surround position
                            move.SetLocation(enemyTransform.Position);
                        }
                        if (move.DistanceRemaining < 4.0f && !state.AttackNow)
                        {
                            state.AttackDelay -= DeltaTime;
                        }         else if (state.AttackNow && move.CanMove)
                        {
                            Debug.Log("We Attack AT DAWN");
                        }
                    }
                }
            }

        }


    }
    
    [UpdateInGroup(typeof(IAUSUpdateStateGroup))]

    public partial struct MagicMeleeTagReactor : IComponentReactorTagsForAIStates<WeaponSkillAttackTag, AttackState>
    {
        public void ComponentAdded(Entity entity, ref WeaponSkillAttackTag newComponent, ref AttackState AIStateCompoment)
        {

        }

        public void ComponentRemoved(Entity entity, ref AttackState AIStateCompoment, in WeaponSkillAttackTag oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref WeaponSkillAttackTag newComponent, ref AttackState AIStateCompoment, in WeaponSkillAttackTag oldComponent)
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
    [UpdateInGroup(typeof(IAUSUpdateStateGroup))]

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