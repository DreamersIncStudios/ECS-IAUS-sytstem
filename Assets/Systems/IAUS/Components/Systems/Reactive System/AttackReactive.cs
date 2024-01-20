using Components.MovementSystem;
using IAUS.ECS.Component;
using IAUS.ECS.Component.Aspects;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Utilities.ReactiveSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<AttackActionTag, AttackState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AttackActionTag, AttackState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AttackActionTag, AttackState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.ManageComponentRemovalJob))]



namespace IAUS.ECS.Systems.Reactive
{
    public partial struct AttackTagReactor : IComponentReactorTagsForAIStates<AttackActionTag, AttackState>
    {
        public void ComponentAdded(Entity entity, ref AttackActionTag newComponent, ref AttackState AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;

        }

        public void ComponentRemoved(Entity entity, ref AttackState AIStateCompoment, in AttackActionTag oldComponent)
        {
            AIStateCompoment.Status = ActionStatus.CoolDown;
            AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
        }

        public void ComponentValueChanged(Entity entity, ref AttackActionTag newComponent,
            ref AttackState AIStateCompoment, in AttackActionTag oldComponent)
        {
            Debug.Log("Change");
        }

        public partial class ReactiveSystem : AIReactiveSystemBase<AttackActionTag, AttackState, AttackTagReactor>
        {
            protected override AttackTagReactor CreateComponentReactor()
            {
                return new AttackTagReactor();
            }

        }
        
        [UpdateAfter(typeof(InitializationSystemGroup))]
        public partial class AttackUpdateSystem : SystemBase
        {
            EntityQuery attackTagAdd;
            private EntityQuery attackTagRemoved;

            protected override void OnCreate()
            {
                attackTagAdd =
                    GetEntityQuery(new EntityQueryDesc()
                    {
                        All = new[]
                        {
                            ComponentType.ReadWrite(typeof(AttackState)),
                            ComponentType.ReadWrite(typeof(AttackActionTag)),
                            ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(LocalTransform))
                        },
                        Absent = new[]
                        {
                            ComponentType.ReadOnly(
                                typeof(AIReactiveSystemBase<AttackActionTag, AttackState, AttackTagReactor>.
                                    StateComponent)),
                            ComponentType.ReadOnly(typeof(MeleeAttackTag)),
                            ComponentType.ReadOnly(typeof(MagicAttackTag)),
                            ComponentType.ReadOnly(typeof(WeaponSkillAttackTag)),
                            ComponentType.ReadOnly(typeof(RangeAttackTag))
                        }
                    });
                attackTagRemoved =
                    GetEntityQuery(new EntityQueryDesc()
                    { 
                        Absent= new[]
                        {
                            ComponentType.ReadWrite(typeof(AttackActionTag)),
                        },
                        Any = new[]{
                            ComponentType.ReadOnly(typeof(MeleeAttackTag)),
                            ComponentType.ReadOnly(typeof(MagicAttackTag)),
                            ComponentType.ReadOnly(typeof(WeaponSkillAttackTag)),
                            ComponentType.ReadOnly(typeof(RangeAttackTag))
                        },
                        All = new[]{ComponentType.ReadOnly(typeof(AttackState))}
                    });
            }

            protected override void OnUpdate()
            {
                var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
                new GetHighestSubState().ScheduleParallel();
                new DefineAttackType() { ecb = ecb.CreateCommandBuffer(World.Unmanaged).AsParallelWriter() }
                    .ScheduleParallel(attackTagAdd);
                new RemoveExtraTags() { ecb = ecb.CreateCommandBuffer(World.Unmanaged).AsParallelWriter() }
                    .ScheduleParallel(attackTagRemoved);
            }

            partial struct GetHighestSubState : IJobEntity
            {
                void Execute(AttackAspect aspect, ref AttackActionTag tag)
                {
                    tag.SubStateNumber = aspect.HighState;
                }

            }

            partial struct DefineAttackType : IJobEntity
            {
                public EntityCommandBuffer.ParallelWriter ecb;

                void Execute(Entity entity, [ChunkIndexInQuery] int sortKey, ref Movement move, ref AttackActionTag tag)
                {
                    move.CanMove = false;
                    switch (tag.SubStateNumber)
                    {
                        case 0:
                            ecb.AddComponent<MeleeAttackTag>(sortKey, entity);
                            break;
                        case 1:
                            ecb.AddComponent<WeaponSkillAttackTag>(sortKey, entity);
                            break;
                        case 2:
                            ecb.AddComponent<MagicAttackTag>(sortKey, entity);
                            break;
                        case 3:
                            ecb.AddComponent<RangeAttackTag>(sortKey, entity);
                            break;

                    }
                }
            }
            partial struct RemoveExtraTags:IJobEntity
            {
                public EntityCommandBuffer.ParallelWriter ecb;

                void Execute(Entity entity, [ChunkIndexInQuery] int sortKey, ref AttackState state)
                {
                    ecb.RemoveComponent<MeleeAttackTag>(sortKey, entity);
                    ecb.RemoveComponent<WeaponSkillAttackTag>(sortKey, entity);
                    ecb.RemoveComponent<MagicAttackTag>(sortKey, entity);
                    ecb.RemoveComponent<RangeAttackTag>(sortKey, entity);

                }
            }
        }
    }
}