using System;
using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Components.MovementSystem;
using DreamersInc.QuadrantSystems;
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
                temp.ResetTime = 15;
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
            private EntityQuery wanderingStopped;


            protected override void OnCreate()
            {
                componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[]
                    {
                        ComponentType.ReadWrite(typeof(WanderActionTag)),
                        ComponentType.ReadWrite(typeof(Movement)),
                        ComponentType.ReadOnly(typeof(LocalToWorld)),
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
                        ComponentType.ReadOnly(typeof(LocalToWorld)),
                        ComponentType.ReadOnly(typeof(Movement)),
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
                var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

                var depends = Dependency;
                depends = new WanderSetupJob()
                {
                }.Schedule(componentAddedQuery, depends);
                depends = new WanderStopJob()
                {
                }.Schedule(wanderingStopped, depends);
                depends = new WanderPlanner()
                {
                    ECB=  ecbSingleton.CreateCommandBuffer(World.Unmanaged).AsParallelWriter(),
                    deltaTime = SystemAPI.Time.DeltaTime,
                    
                }.Schedule(depends);
                Dependency = depends;
            }

            [BurstCompile]
            public partial struct WanderSetupJob : IJobEntity
            {
                void Execute(ref WanderActionTag wander, [ReadOnly] LocalToWorld transform)
                {
                    wander.HashKey = NPCQuadrantSystem.GetPositionHashMapKey(transform.Position);
                }
            }

            [BurstCompile]
            public partial struct WanderStopJob : IJobEntity
            {

                void Execute(ref Movement move)
                {
                    move.CanMove = false;
                }
            }
                        

            public partial struct WanderPlanner : IJobEntity
            {
                public EntityCommandBuffer.ParallelWriter ECB;
                public float deltaTime;
                private void Execute(Entity entity, [ChunkIndexInQuery] int sortkey, ref WanderActionTag wander, ref Movement move)
                {
                    if (move.DistanceRemaining < 5.5 && wander.WaitTimer > 00.0f && wander.Plan != TravelPlan.Wait)
                    {
                        wander.Plan = TravelPlan.Wait;
                        ExecutePlan(entity, sortkey,ref wander, ref move);
                    }
                    if (move.DistanceRemaining < 5.5 && wander.WaitTimer == 00.0f && wander.Plan != TravelPlan.GetNewLocation)
                    {
                        wander.Plan = TravelPlan.GetNewLocation;
                        ExecutePlan(entity, sortkey,ref wander, ref move);
                        
                    }

                    if (move.DistanceRemaining > 5.5 && wander.WaitTimer == 00.0f && wander.Plan != TravelPlan.MoveToLocation)
                    {
                        wander.Plan = TravelPlan.MoveToLocation;
                        ExecutePlan(entity, sortkey,ref wander, ref move);
                        wander.WaitTimer = 10;

                    }
                    if(wander is { WaitTimer: > 00.0f, Plan: TravelPlan.Wait })
                        wander.WaitTimer -= deltaTime;
                    if (wander.WaitTimer <= 00.0f)
                        wander.WaitTimer = 00.0f;
                        
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
              
                    }
                    
                }
            }

        }

    }
}