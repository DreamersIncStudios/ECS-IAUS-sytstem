using System.Collections.Generic;
using System.Linq;
using IAUS.ECS.Component;
using IAUS.ECS.Component.Attacking;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utilities.ReactiveSystem;


[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<TerrorizeAreaTag, TerrorizeAreaState, IAUS.ECS.Systems.Reactive.TerrorTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<TerrorizeAreaTag, TerrorizeAreaState, IAUS.ECS.Systems.Reactive.TerrorTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<TerrorizeAreaTag, TerrorizeAreaState, IAUS.ECS.Systems.Reactive.TerrorTagReactor>.ManageComponentRemovalJob))]


namespace IAUS.ECS.Systems.Reactive
{

    public partial struct TerrorTagReactor : IComponentReactorTagsForAIStates<TerrorizeAreaTag, TerrorizeAreaState>
    {

        public void ComponentAdded(Entity entity, ref TerrorizeAreaTag newComponent,
            ref TerrorizeAreaState aiStateComponent)
        {
            aiStateComponent.Status = ActionStatus.Running;
            aiStateComponent.TargetPosition = float3.zero;
        }

        public void ComponentRemoved(Entity entity, ref TerrorizeAreaState aiStateComponent,
            in TerrorizeAreaTag oldComponent)
        {
            aiStateComponent.Status = ActionStatus.CoolDown;
            aiStateComponent.ResetTime = aiStateComponent.CoolDownTime;
        }

        public void ComponentValueChanged(Entity entity, ref TerrorizeAreaTag newComponent,
            ref TerrorizeAreaState aiStateComponent,
            in TerrorizeAreaTag oldComponent)
        {
        }

        partial class ReactiveSystem : AIReactiveSystemBase<TerrorizeAreaTag, TerrorizeAreaState, TerrorTagReactor>
        {
            protected override TerrorTagReactor CreateComponentReactor()
            {
                return new TerrorTagReactor();
            }
        }


        public partial class TerrorizeUpdateSystem : SystemBase
        {
            private BeginSimulationEntityCommandBufferSystem.Singleton ecb;
            

            protected override void OnUpdate()
            {
                ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

                var depends = Dependency;
                
                depends = new DetermineAction()
                {
                    DeltaTime = SystemAPI.Time.DeltaTime,
                    ECB = ecb.CreateCommandBuffer(World.Unmanaged).AsParallelWriter(),
                }.Schedule(depends);
                depends = new GetTerrorPosition().Schedule(depends);
                
                Dependency = depends;

            }

            partial struct DetermineAction : IJobEntity
            {
                public float DeltaTime;
                public EntityCommandBuffer.ParallelWriter ECB;

                void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, TerrorizeAreaAspect aspect,
                    in TerrorizeAreaTag tag)
                {
                    aspect.DeterminePlan();
                    aspect.ExecutePlan(entity, chunkIndex, DeltaTime, ECB);
                }
            }
            
            
            partial struct GetTerrorPosition: IJobEntity
            {
                void Execute([ChunkIndexInQuery] int chunkIndex, ref TerrorizeAreaState state, InteractablesAspect aspect, in TerrorizeAreaTag tag)
                {
                    if(state.AttackPlans.IsEmpty) return;
                    if (state.AttackPlans[0] != AttackPlan.GetAttackLocation)
                        return;
                    state.TargetEntity = aspect.closestInteractable.Entity;
                    state.TargetPosition = aspect.closestInteractable.Position;
                    
                    state.AttackPlans.RemoveAt(0);
                }

            }
        }
    }
}