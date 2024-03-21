using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AISenses;
using Components.MovementSystem;
using IAUS.ECS.Component;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Utilities.ReactiveSystem;



[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<ChaseTargetTag, PursueTarget, IAUS.ECS.Systems.Reactive.PursueReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<ChaseTargetTag, PursueTarget, IAUS.ECS.Systems.Reactive.PursueReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<ChaseTargetTag, PursueTarget, IAUS.ECS.Systems.Reactive.PursueReactor>.ManageComponentRemovalJob))]



namespace IAUS.ECS.Systems.Reactive
{
    public partial struct PursueReactor : IComponentReactorTagsForAIStates<ChaseTargetTag, PursueTarget>
    {
        public void ComponentAdded(Entity entity, ref ChaseTargetTag newComponent, ref PursueTarget AIStateCompoment)
        {        
            AIStateCompoment.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref PursueTarget AIStateCompoment, in ChaseTargetTag oldComponent)
        {
            
            if(AIStateCompoment.Status != ActionStatus.Success)
                AIStateCompoment.TargetEntity = Entity.Null;
            AIStateCompoment.Status = ActionStatus.CoolDown;
            AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
        }

        public void ComponentValueChanged(Entity entity, ref ChaseTargetTag newComponent, ref PursueTarget AIStateCompoment,
            in ChaseTargetTag oldComponent)
        {
            Debug.Log("Change");
        }
        public partial class ReactiveSystem : AIReactiveSystemBase<ChaseTargetTag, PursueTarget, PursueReactor>
        {
            protected override PursueReactor CreateComponentReactor()
            {
                return new PursueReactor();
            }

        }
    }
    [UpdateAfter(typeof(InitializationSystemGroup))]

    public partial class ChaseGroup : ComponentSystemGroup
    {
        public ChaseGroup()
        {
            RateManager = new RateUtils.VariableRateManager(500, true);

        }

    }
    [UpdateAfter(typeof(ChaseGroup))]
    public partial class PursueUpdateSystem : SystemBase
    {
        private EntityQuery chaseAdded;
        protected override void OnCreate()
        {
            chaseAdded = GetEntityQuery(new EntityQueryDesc()
                {
                    All = new[]
                    {
                        ComponentType.ReadWrite(typeof(PursueTarget)),
                        ComponentType.ReadWrite(typeof(ChaseTargetTag)),
                        ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(LocalTransform))
                    },
                    Absent = new[]
                    {
                        ComponentType.ReadOnly(
                            typeof(AIReactiveSystemBase<ChaseTargetTag, PursueTarget, PursueReactor>.
                                StateComponent))
      
                    }
                });
        }

        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();

            new ChaseTarget()
            {
                ecb = ecb.CreateCommandBuffer(World.DefaultGameObjectInjectionWorld.Unmanaged)
            }.Schedule();
        }

        partial struct ChaseTarget: IJobEntity
        {
            public EntityCommandBuffer ecb;
            void Execute(Entity entity,ref Movement move, ref PursueTarget state,ref DynamicBuffer<ScanPositionBuffer> buffer, in ChaseTargetTag tag)
            {

                if (buffer.Length == 0)
                {
                    move.CanMove = false;
                    return;
                }

                if (state.TargetEntity == Entity.Null)
                {
                    foreach (var target in buffer)
                    {
                        if (!target.target.IsFriendly)
                        {
                            state.TargetEntity = target.target.Entity;
                            state.TargetArea = target.target.LastKnownPosition;
                            return;
                        }
                        
                    }
                }
                else
                {

                    foreach (var target in buffer)
                    {
                        if (target.target.Entity != state.TargetEntity) continue;
                        state.TargetArea = target.target.LastKnownPosition;
                        return;
                    }
                }

                move.SetLocation(state.TargetArea);
                if (move.DistanceRemaining <= 2.5f)
                    state.Status = ActionStatus.Success;
            }

        }
    }
}