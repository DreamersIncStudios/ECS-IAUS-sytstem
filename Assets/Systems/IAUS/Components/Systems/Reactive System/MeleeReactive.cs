using System.Collections.Generic;
using Components.MovementSystem;
using IAUS.ECS.Component;
using DreamersInc.ComboSystem;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Utilities.ReactiveSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, IAUS.ECS.Systems.Reactive.MeleeTagReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive {
    [UpdateInGroup(typeof(IAUSUpdateStateGroup))]
    public partial struct MeleeTagReactor : IComponentReactorTagsForAIStates<MeleeAttackTag, MeleeAttackSubState>
    {
        public void ComponentAdded(Entity entity, ref MeleeAttackTag newComponent,
            ref MeleeAttackSubState AIStateCompoment)
        {
        }

        public void ComponentRemoved(Entity entity, ref MeleeAttackSubState AIStateCompoment,
            in MeleeAttackTag oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref MeleeAttackTag newComponent,
            ref MeleeAttackSubState AIStateCompoment, in MeleeAttackTag oldComponent)
        {

        }
        [UpdateInGroup(typeof(IAUSUpdateStateGroup))]

        partial class ReactiveSystem : AIReactiveSystemBase<MeleeAttackTag, MeleeAttackSubState, MeleeTagReactor>
        {
            protected override MeleeTagReactor CreateComponentReactor()
            {
                return new MeleeTagReactor();
            }

        }

        [UpdateInGroup(typeof(IAUSUpdateStateGroup))]

        [UpdateAfter(typeof(AttackTagReactor.AttackUpdateSystem))]
        partial class MeleeReactiveSystem : SystemBase
        {
        
            protected override void OnUpdate()
            {
                var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
                
                new AttackTargetEnemy()
                {
                    DeltaTime = SystemAPI.Time.DeltaTime,
                    ECB = ecb.CreateCommandBuffer(World.Unmanaged).AsParallelWriter()
                }.ScheduleParallel();
                
                Entities.WithoutBurst().WithStructuralChanges().ForEach(
                    (Entity entity,Command handler,Animator anim, NPCAttack comboList,  in SelectAndAttack select,in MeleeAttackSubState meleeAttackersAdded) =>
                    {
                        handler.InputQueue ??= new Queue<AnimationTrigger>();
                       if(anim.IsInTransition(0))return;
                           handler.InputQueue.Enqueue(comboList.AttackSequence.PickAttack(IAttackSequence.AttackType.Melee)[0]);
                           EntityManager.RemoveComponent<SelectAndAttack>(entity);
                    }).Run();

            }


[BurstCompile]
            private partial struct AttackTargetEnemy : IJobEntity
            {
                public float DeltaTime;
                public EntityCommandBuffer.ParallelWriter ECB;
                void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref Movement move,
                    ref MeleeAttackSubState state, ref MeleeAttackTag tag,
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
                        ECB.AddComponent<SelectAndAttack>(chunkIndex,entity);
                    }
                }
            }

        }


    }

    public struct SelectAndAttack : IComponentData
    {
    }
    public class NPCAttack : IComponentData
    {
        public NPCAttackSequence AttackSequence;
    }
}