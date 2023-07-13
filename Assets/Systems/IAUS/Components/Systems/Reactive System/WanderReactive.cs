using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using Components.MovementSystem;
using Unity.Burst.Intrinsics;
using Unity.Mathematics;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<WanderActionTag, WanderQuadrant, IAUS.ECS.Systems.Reactive.WanderTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<WanderActionTag, WanderQuadrant, IAUS.ECS.Systems.Reactive.WanderTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<WanderActionTag, WanderQuadrant, IAUS.ECS.Systems.Reactive.WanderTagReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive
{
    public partial struct WanderTagReactor : IComponentReactorTagsForAIStates<WanderActionTag, WanderQuadrant>
    {
        public void ComponentAdded(Entity entity, ref WanderActionTag newComponent, ref WanderQuadrant AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
            newComponent.WaitTime = 10;
        }

        public void ComponentRemoved(Entity entity, ref WanderQuadrant AIStateCompoment, in WanderActionTag oldComponent)
        {
            if (AIStateCompoment.Complete || AIStateCompoment.Status == ActionStatus.Success)
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
            }
            else
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime * 2;

            }
        }

        public void ComponentValueChanged(Entity entity, ref WanderActionTag newComponent, ref WanderQuadrant AIStateCompoment, in WanderActionTag oldComponent)
        {
            throw new System.NotImplementedException();
        }

        public partial class WanderReactiveSystem : AIReactiveSystemBase<WanderActionTag, WanderQuadrant, WanderTagReactor>
        {
            protected override WanderTagReactor CreateComponentReactor()
            {
                return new WanderTagReactor();
            }

        }

        public partial class WanderSystem : SystemBase
        {
            private EntityQuery componentAddedQuery;
            private EntityQuery wandering ;

            EntityCommandBufferSystem entityCommandBufferSystem;
            protected override void OnCreate()
            {
                base.OnCreate();
                componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[] { ComponentType.ReadWrite(typeof(WanderQuadrant)), ComponentType.ReadWrite(typeof(WanderActionTag)), ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(LocalTransform))
                },
                    None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<WanderActionTag, WanderQuadrant, WanderTagReactor>.StateComponent)) }
                });
                wandering  = GetEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[] { ComponentType.ReadWrite(typeof(WanderQuadrant)), ComponentType.ReadWrite(typeof(WanderActionTag)), ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(LocalTransform)),
                    ComponentType.ReadOnly(typeof(AIReactiveSystemBase<WanderActionTag, WanderQuadrant, WanderTagReactor>.StateComponent)) }
                });

                entityCommandBufferSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();

            }
            protected override void OnUpdate()
            {
                var systemDeps = Dependency;
                systemDeps = new WanderSetupJob() { }.ScheduleParallel(componentAddedQuery, systemDeps);

                Dependency = systemDeps;
            }
            public partial struct WanderSetupJob : IJobEntity {
                public EntityCommandBuffer.ParallelWriter ecb;
                public void Execute(ref WanderQuadrant wander, ref Movement move, [ReadOnly]LocalTransform transform) {
                    wander.StartingDistance = Vector3.Distance(transform.Position, wander.TravelPosition);

                    move.SetLocation(wander.TravelPosition);

                }

              
            }
        }
    }
}
