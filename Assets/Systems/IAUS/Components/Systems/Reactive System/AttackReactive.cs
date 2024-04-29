using System.Collections.Generic;
using Components.MovementSystem;
using DreamersInc.ComboSystem;
using IAUS.ECS.Component;
using Unity.Collections;
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
        public void ComponentAdded(Entity entity, ref AttackActionTag newComponent, ref AttackState aiStateComponent)
        {
            aiStateComponent.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref AttackState aiStateComponent, in AttackActionTag oldComponent)
        {
            aiStateComponent.Status = ActionStatus.CoolDown;
            aiStateComponent.ResetTime = aiStateComponent.CoolDownTime;
        }

        public void ComponentValueChanged(Entity entity, ref AttackActionTag newComponent,
            ref AttackState aiStateComponent, in AttackActionTag oldComponent)
        {
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
            private EntityQuery attackTagAdd;
            private EntityQuery attackTagRemoved;
            private BeginSimulationEntityCommandBufferSystem.Singleton ecb;
            protected override void OnCreate()
            {
                attackTagAdd = GetEntityQuery(new EntityQueryDesc()
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
                                StateComponent))
                    }
                });
                attackTagRemoved = GetEntityQuery(new EntityQueryDesc()
                {
                    Absent = new[]
                    {
                        ComponentType.ReadWrite(typeof(AttackActionTag)),
                    },
                    All = new[] { ComponentType.ReadOnly(typeof(AttackState)) }
                });
                 ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            }

            protected override void OnUpdate()
            {
               
                new DetermineAction()
                {
                    deltaTime = SystemAPI.Time.DeltaTime,
                    ECB = ecb.CreateCommandBuffer(World.Unmanaged).AsParallelWriter()
                }.Schedule();
                
                Entities.WithoutBurst().WithStructuralChanges().ForEach(
                    (Entity entity, Command handler, Animator anim, NPCAttack comboList, in SelectAndAttack select) =>
                    {
                        handler.InputQueue ??= new Queue<AnimationTrigger>();
                        if (anim.IsInTransition(0)) return;
                        /*   handler.InputQueue.Enqueue(
                               comboList.AttackSequence.PickAttack(IAttackSequence.AttackType.Melee)[0]);
                          */
                        EntityManager.RemoveComponent<SelectAndAttack>(entity);
                        Debug.Log("attacked");
                    }).Run();
            }

            partial struct DetermineAction: IJobEntity
            {
                public float deltaTime;
                public EntityCommandBuffer.ParallelWriter ECB;
                void Execute([ChunkIndexInQuery]int chunkIndex, Entity entity, AttackAspect aspect, in AttackActionTag tag)
                {
                    aspect.DeterminePlan();
                    aspect.ExecutePlan(entity, chunkIndex, deltaTime,ECB);
                }
            }

            struct MoveToLocation
            {
                
            }

            struct AttackTarget
            {
            }

            struct CoolDown
            {
                
            }
        }
    }
}