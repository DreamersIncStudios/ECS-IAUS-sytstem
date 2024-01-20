using System.Collections;
using System.Collections.Generic;
using AISenses.VisionSystems.Combat;
using Components.MovementSystem;
using IAUS.ECS.Component;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Utilities.ReactiveSystem;


[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<MagicAttackTag, MagicAttackSubState, IAUS.ECS.Systems.Reactive.MagicTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MagicAttackTag, MagicAttackSubState, IAUS.ECS.Systems.Reactive.MagicTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MagicAttackTag, MagicAttackSubState, IAUS.ECS.Systems.Reactive.MagicTagReactor>.ManageComponentRemovalJob))]


namespace IAUS.ECS.Systems.Reactive
{

    public partial struct MagicTagReactor : IComponentReactorTagsForAIStates<MagicAttackTag, MagicAttackSubState>
    {
        public void ComponentAdded(Entity entity, ref MagicAttackTag newComponent, ref MagicAttackSubState AIStateCompoment)
        {
        }

        public void ComponentRemoved(Entity entity, ref MagicAttackSubState AIStateCompoment, in MagicAttackTag oldComponent)
        {
        }

        public void ComponentValueChanged(Entity entity, ref MagicAttackTag newComponent, ref MagicAttackSubState AIStateCompoment,
            in MagicAttackTag oldComponent)
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
    [UpdateInGroup(typeof(IAUSUpdateStateGroup))]

    [UpdateAfter(typeof(AttackTagReactor.AttackUpdateSystem))]
    partial class MagicAttackReactive : SystemBase
    {
        private EntityQuery magicAttackersRemoved;
        private EntityQuery magicAttackersAdded;

        protected override void OnCreate()
        {
            magicAttackersAdded = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadWrite(typeof(MagicAttackSubState)),
                    ComponentType.ReadWrite(typeof(AttackState)), ComponentType.ReadWrite(typeof(MeleeAttackTag)),
                    ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(LocalTransform)),
                    ComponentType.ReadOnly(typeof(AttackTarget))
                },
                Absent = new[]
                {
                    ComponentType.ReadOnly(
                        typeof(AIReactiveSystemBase<MagicAttackTag, MagicAttackSubState, MagicTagReactor>.
                            StateComponent))
                }

            });
            magicAttackersRemoved = GetEntityQuery(new EntityQueryDesc()
            {
                All = new[]
                {
                    ComponentType.ReadWrite(typeof(MagicAttackSubState)),
                    ComponentType.ReadWrite(typeof(AttackState)),
                    ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(LocalTransform)),
                    ComponentType.ReadOnly(
                        typeof(AIReactiveSystemBase<MagicAttackTag, MagicAttackSubState, MagicTagReactor>.
                            StateComponent))
                },
                Absent = new[] { ComponentType.ReadWrite(typeof(MagicAttackTag)) }

            });
        }
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
                ref MagicAttackSubState state, ref MagicAttackTag tag,
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
    
