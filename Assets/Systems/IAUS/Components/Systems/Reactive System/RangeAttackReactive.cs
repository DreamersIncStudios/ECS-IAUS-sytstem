using System.Collections;
using System.Collections.Generic;
using Components.MovementSystem;
using IAUS.ECS.Component;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Utilities.ReactiveSystem;


[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<RangeAttackTag, RangedAttackSubState, IAUS.ECS.Systems.Reactive.RangeTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RangeAttackTag, RangedAttackSubState, IAUS.ECS.Systems.Reactive.RangeTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RangeAttackTag, RangedAttackSubState, IAUS.ECS.Systems.Reactive.RangeTagReactor>.ManageComponentRemovalJob))]


namespace IAUS.ECS.Systems.Reactive
{

    public partial struct RangeTagReactor : IComponentReactorTagsForAIStates<RangeAttackTag, RangedAttackSubState>
    {
        public void ComponentAdded(Entity entity, ref RangeAttackTag newComponent, ref RangedAttackSubState AIStateCompoment)
        {
        }

        public void ComponentRemoved(Entity entity, ref RangedAttackSubState AIStateCompoment, in RangeAttackTag oldComponent)
        {
        }

        public void ComponentValueChanged(Entity entity, ref RangeAttackTag newComponent, ref RangedAttackSubState AIStateCompoment,
            in RangeAttackTag oldComponent)
        {
        }
        [UpdateInGroup(typeof(IAUSUpdateStateGroup))]

        partial class ReactiveSystem : AIReactiveSystemBase<RangeAttackTag, RangedAttackSubState, RangeTagReactor>
        {
            protected override RangeTagReactor CreateComponentReactor()
            {
                return new RangeTagReactor();
            }

        }
        
    }

    partial class RangeAttackReactive : SystemBase
    {

        protected override void OnUpdate()
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
                
            new AttackTargetEnemy()
            {
                Seed = (uint)UnityEngine.Random.Range(1, 10000),
                DeltaTime = SystemAPI.Time.DeltaTime,
                    
                ECB = ecb.CreateCommandBuffer(World.Unmanaged).AsParallelWriter()
            }.ScheduleParallel();

        }
        [BurstCompile]
        private partial struct AttackTargetEnemy : IJobEntity
        {
            public float DeltaTime;
            public uint Seed;
            public EntityCommandBuffer.ParallelWriter ECB;
            void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref Movement move,
                ref RangedAttackSubState state, ref RangeAttackTag tag,
                in LocalTransform transform)
            {
                var dist = Vector3.Distance(state.AttackTargetLocation, transform.Position);
                if (dist > 10.5f)
                {
                    //TODO select a surround position
                    move.SetLocation(state.AttackTargetLocation);
                    state.AttackDelay = 10;
                }

                if (move.DistanceRemaining < 4.0f && !state.AttackNow)
                {
                    state.AttackDelay -= DeltaTime;
                }
                else if (state.AttackNow && move.CanMove)
                {
                    state.AttackDelay += 10.5f;
                }
            }
        }
    }
}