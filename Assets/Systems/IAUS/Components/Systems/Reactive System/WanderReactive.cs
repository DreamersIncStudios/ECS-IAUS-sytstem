using System;
using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Components.MovementSystem;
using Unity.Burst;
using Random = UnityEngine.Random;

[assembly:
    RegisterGenericComponentType(
        typeof(AIReactiveSystemBuffer<WanderActionTag, StateData, IAUS.ECS.Systems.Reactive.WanderTagReactor>.
            StateComponent))]
[assembly:
    RegisterGenericJobType(
        typeof(AIReactiveSystemBuffer<WanderActionTag, StateData, IAUS.ECS.Systems.Reactive.WanderTagReactor>.
            ManageComponentAdditionJob))]
[assembly:
    RegisterGenericJobType(
        typeof(AIReactiveSystemBuffer<WanderActionTag, StateData, IAUS.ECS.Systems.Reactive.WanderTagReactor>.
            ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive
{
    public partial struct WanderTagReactor : IComponentReactorTagsForAIBuffer<WanderActionTag, StateData>
    {
        public void ComponentAdded(Entity entity, ref WanderActionTag newAITag, DynamicBuffer<StateData> AIStateCompoment)
        {
            for (int i = 0; i < AIStateCompoment.Length; i++)
            {
                if (AIStateCompoment[i].State != AIStates.WanderQuadrant)
                    continue;
                var temp = AIStateCompoment[i];
                temp.SetStatus( ActionStatus.Running);
                AIStateCompoment[i] = temp;
            }
            newAITag.WaitTime = 10;
        }

        public void ComponentRemoved(Entity entity, DynamicBuffer<StateData> AIStateCompoment,
            in WanderActionTag oldComponent)
        {
            for (int i = 0; i < AIStateCompoment.Length; i++)
            {
                if (AIStateCompoment[i].State != AIStates.WanderQuadrant)
                    continue;
                var temp = AIStateCompoment[i];
                temp.SetStatus( ActionStatus.Success);
                AIStateCompoment[i] = temp;
            }
            // if (AIStateCompoment.Complete || AIStateCompoment.Status == ActionStatus.Success)
            // {
            //     AIStateCompoment.Status = ActionStatus.CoolDown;
            //     AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
            // }
            // else
            // {
            //     AIStateCompoment.Status = ActionStatus.CoolDown;
            //     AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime * 2;
            // }
        }
 



        public partial class
            WanderReactiveSystem : AIReactiveSystemBuffer<WanderActionTag, StateData, WanderTagReactor>
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
                            typeof(AIReactiveSystemBuffer<WanderActionTag, StateData, WanderTagReactor>.
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
                            typeof(AIReactiveSystemBuffer<WanderActionTag, StateData, WanderTagReactor>.
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
                        

            public partial struct WanderPlanner : IJobEntity
            {
                public EntityCommandBuffer.ParallelWriter ECB;
                private void Execute(Entity entity, [ChunkIndexInQuery] int sortkey, ref WanderActionTag wander, ref Movement move)
                {
                    if (move.DistanceRemaining < 1.5 && wander.WaitTimer > 00.0f && wander.Plan != TravelPlan.Wait)
                    {
                        wander.Plan = TravelPlan.Wait;
                        ExecutePlan(entity, sortkey,ref wander, ref move);
                    }
                    if (move.DistanceRemaining < 1.5 && wander.WaitTimer == 00.0f && wander.Plan != TravelPlan.GetNewLocation)
                    {
                        wander.Plan = TravelPlan.GetNewLocation;
                    }

                    if (move.DistanceRemaining > 1.5 && wander.WaitTimer == 00.0f && wander.Plan != TravelPlan.MoveToLocation)
                    {
                        wander.Plan = TravelPlan.MoveToLocation;
                    }
                }

                void ExecutePlan(Entity entity, [ChunkIndexInQuery] int sortkey,ref WanderActionTag wander, ref Movement move)
                {
                    switch (wander.Plan)
                    {
                        case TravelPlan.none:
                            break;
                        case TravelPlan.GetNewLocation:
                            
                            ECB.AddComponent(sortkey, entity, new UpdateWanderLocationTag());
                            break;
                        case TravelPlan.MoveToLocation:
                            move.SetLocation(wander.TravelPosition);
                            break;
                        case TravelPlan.Wait:
                            wander.WaitTimer = Random.Range(9, 25);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

        }

    }
}