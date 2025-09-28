using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Components.MovementSystem;
using Unity.Burst;

[assembly:
    RegisterGenericComponentType(
        typeof(AIReactiveSystemBase<WanderActionTag, WanderQuadrant, IAUS.ECS.Systems.Reactive.WanderTagReactor>.
            StateComponent))]
[assembly:
    RegisterGenericJobType(
        typeof(AIReactiveSystemBase<WanderActionTag, WanderQuadrant, IAUS.ECS.Systems.Reactive.WanderTagReactor>.
            ManageComponentAdditionJob))]
[assembly:
    RegisterGenericJobType(
        typeof(AIReactiveSystemBase<WanderActionTag, WanderQuadrant, IAUS.ECS.Systems.Reactive.WanderTagReactor>.
            ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive
{
    public partial struct WanderTagReactor : IComponentReactorTagsForAIStates<WanderActionTag, WanderQuadrant>
    {
        public void ComponentAdded(Entity entity, ref WanderActionTag newComponent, ref WanderQuadrant AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
            newComponent.WaitTime = 10;
        }

        public void ComponentRemoved(Entity entity, ref WanderQuadrant AIStateCompoment,
            in WanderActionTag oldComponent)
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

        public void ComponentValueChanged(Entity entity, ref WanderActionTag newComponent,
            ref WanderQuadrant AIStateCompoment, in WanderActionTag oldComponent)
        {
        }

        public partial class
            WanderReactiveSystem : AIReactiveSystemBase<WanderActionTag, WanderQuadrant, WanderTagReactor>
        {
            protected override WanderTagReactor CreateComponentReactor()
            {
                return new WanderTagReactor();
            }
        }

        public partial class WanderSystem : SystemBase
        {
            private EntityQuery componentAddedQuery;
            private ComponentLookup<Movement> mover;
            private EntityQuery wanderingStopped;


            protected override void OnCreate()
            {
                componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadWrite(typeof(WanderQuadrant)),
                        ComponentType.ReadWrite(typeof(WanderActionTag)),
                        ComponentType.ReadOnly(typeof(LocalTransform)),
                        ComponentType.ReadOnly(typeof(Parent))
                    },
                    Absent = new ComponentType[]
                    {
                        ComponentType.ReadOnly(
                            typeof(AIReactiveSystemBase<WanderActionTag, WanderQuadrant, WanderTagReactor>.
                                StateComponent))
                    }
                });
                wanderingStopped = GetEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadWrite(typeof(WanderQuadrant)),
                        ComponentType.ReadOnly(typeof(LocalTransform)),
                        ComponentType.ReadOnly(typeof(Parent)),
                        ComponentType.ReadOnly(
                            typeof(AIReactiveSystemBase<WanderActionTag, WanderQuadrant, WanderTagReactor>.
                                StateComponent))
                    },
                    Absent = new ComponentType[] { ComponentType.ReadWrite(typeof(WanderActionTag)) },
                });
            }

            protected override void OnUpdate()
            {
                var depends = Dependency;
                mover = GetComponentLookup<Movement>(false);
                depends = new WanderSetupJob()
                {
                    Movements = mover
                }.Schedule(componentAddedQuery, depends);
                depends = new WanderStopJob()
                {
                    Movements = mover
                }.Schedule(wanderingStopped, depends);
                Dependency = depends;
            }

            [BurstCompile]
            public partial struct WanderSetupJob : IJobEntity
            {
                public ComponentLookup<Movement> Movements;

                void Execute(ref WanderQuadrant wander, in Parent parent, [ReadOnly] LocalTransform transform)
                {
                    wander.StartingDistance = Vector3.Distance(transform.Position, wander.TravelPosition);
                    var test = Movements[parent.Value];
                    test.SetLocation(wander.TravelPosition);
                    Movements[parent.Value] = test;

                    Debug.Log("test");
                }
            }

            [BurstCompile]
            public partial struct WanderStopJob : IJobEntity
            {
                public ComponentLookup<Movement> Movements;

                void Execute(ref WanderQuadrant wander, in Parent parent)
                {
                    var move = Movements[parent.Value];
                    move.CanMove = false;
                    Movements[parent.Value] = move;
                }
            }
        }
    }
}