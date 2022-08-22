using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using UnityEngine.AI;
using Components.MovementSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, IAUS.ECS.Systems.Reactive.RetreatCitizenTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, IAUS.ECS.Systems.Reactive.RetreatCitizenTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, IAUS.ECS.Systems.Reactive.RetreatCitizenTagReactor>.ManageComponentRemovalJob))]


namespace IAUS.ECS.Systems.Reactive
{
    public struct RetreatCitizenTagReactor : IComponentReactorTagsForAIStates<RetreatActionTag, RetreatCitizen>
    {
        public void ComponentAdded(Entity entity, ref RetreatActionTag newComponent, ref RetreatCitizen AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref RetreatCitizen AIStateCompoment, in RetreatActionTag oldComponent)
        {

                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
        }

        public void ComponentValueChanged(Entity entity, ref RetreatActionTag newComponent, ref RetreatCitizen AIStateCompoment, in RetreatActionTag oldComponent)
        {
        }

        public class RetreatReactiveSystem : AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>
        {
            protected override RetreatCitizenTagReactor CreateComponentReactor()
            {
                return new RetreatCitizenTagReactor();
            }
        }
    }

    [UpdateInGroup(typeof(IAUSUpdateGroup))]

    public sealed partial class RetreatMovement : SystemBase
    {
        private EntityQuery _componentAddedQuery;
        private EntityQuery _componentAddedQueryWithWait;

        private EntityQuery _componentRemovedQuery;
        EntityCommandBufferSystem _entityCommandBufferSystem;

    
        protected override void OnCreate()
        {
            base.OnCreate();
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)),  ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>.StateComponent)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(RetreatActionTag)) }

            });


            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {

            JobHandle systemDeps = Dependency;

            Dependency = systemDeps;

        }
       

    }
    
}