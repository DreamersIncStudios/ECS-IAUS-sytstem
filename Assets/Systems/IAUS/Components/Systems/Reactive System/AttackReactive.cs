using Components.MovementSystem;
using IAUS.ECS.Component;
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
            }

            protected override void OnUpdate()
            {
                throw new System.NotImplementedException();
            }

            partial struct DetermineAction
            {

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