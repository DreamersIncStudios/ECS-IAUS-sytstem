using AISenses.VisionSystems.Combat;
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
            AIStateCompoment.HasAttack = true;
        }

        public void ComponentRemoved(Entity entity, ref AttackState AIStateCompoment, in AttackActionTag oldComponent)
        {
            AIStateCompoment.Status = ActionStatus.CoolDown;
            AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
            AIStateCompoment.HasAttack = false;

        }

        public void ComponentValueChanged(Entity entity, ref AttackActionTag newComponent, ref AttackState AIStateCompoment, in AttackActionTag oldComponent)
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
        public partial class AttackUpdateSystem : SystemBase
        {
            EntityQuery attackTagAdd;
            protected override void OnCreate()
            {
                base.OnCreate();
                attackTagAdd = GetEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackState)), ComponentType.ReadWrite(typeof(AttackActionTag)), ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(LocalTransform)), ComponentType.ReadOnly(typeof(AttackTarget))
                },
                    None = new ComponentType[] { ComponentType.ReadOnly(typeof(MeleeAttackTag)), ComponentType.ReadOnly(typeof(MagicAttackTag)), ComponentType.ReadOnly(typeof(MagicMeleeAttackTag)), ComponentType.ReadOnly(typeof(RangeAttackTag)) }
                });


            }
            protected override void OnUpdate()
            {
                foreach (var (tag, aspect) in SystemAPI.Query<RefRW<AttackActionTag>, AttackAspect>()) {
                    tag.ValueRW.SubStateNumber = aspect.HighState;
                }
                var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
                var tst = new DefineAttackType() { ecb = ecb.CreateCommandBuffer(World.Unmanaged).AsParallelWriter() };
                tst.Schedule(attackTagAdd);
                foreach(var (tag,entity) in SystemAPI.Query<MeleeAttackTag>().WithEntityAccess().WithNone<AttackActionTag>()) {
                    ecb.CreateCommandBuffer(World.Unmanaged).RemoveComponent<MeleeAttackTag>(entity);
                }
            }


            partial struct DefineAttackType : IJobEntity
            {
                public EntityCommandBuffer.ParallelWriter ecb;
                public void Execute(Entity entity, [ChunkIndexInQuery] int sortKey, ref AttackActionTag tag, ref AttackTarget target, AttackState state)
                {
                    state.AttackLocation = target.AttackTargetLocation;
                    switch (tag.SubStateNumber)
                    {
                        case 0:
                            ecb.AddComponent(sortKey, entity, new MeleeAttackTag()
                            {
                                AttackDelay = 6.5f,
                                AttackLocation = target.AttackTargetLocation
                            }) ;
                            break;
                        case 1:
                            ecb.AddComponent<MagicMeleeAttackTag>(sortKey, entity);
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
        }
    }
}