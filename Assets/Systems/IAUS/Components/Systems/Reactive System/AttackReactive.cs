using System.Collections.Generic;
using System.Linq;
using Components.MovementSystem;
using DreamersInc.ComboSystem;
using IAUS.ECS.Component;
using IAUS.ECS.Component.Attacking;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Utilities.ReactiveSystem;


[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<AttackActionTag, AttackState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AttackActionTag, AttackState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AttackActionTag, AttackState, IAUS.ECS.Systems.Reactive.AttackTagReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive
{

    public partial struct AttackTagReactor : IComponentReactorTagsForAIStates<AttackActionTag, AttackState>
    {
        public void ComponentAdded(Entity entity, ref AttackActionTag newComponent, ref AttackState aiStateComponent)
        {
            aiStateComponent.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref AttackState aiStateComponent, in AttackActionTag oldComponent)
        {
            aiStateComponent.Status = ActionStatus.CoolDown;
            aiStateComponent.ResetTime = aiStateComponent.CoolDownTime;
        }

        public void ComponentValueChanged(Entity entity, ref AttackActionTag newComponent,
            ref AttackState aiStateComponent, in AttackActionTag oldComponent)
        {
        }

        public partial class ReactiveSystem : AIReactiveSystemBase<AttackActionTag, AttackState, AttackTagReactor>
        {
            protected override AttackTagReactor CreateComponentReactor()
            {
                return new AttackTagReactor();
            }

        }

        public partial class AttackUpdateSystem : SystemBase
        {
   
            private BeginSimulationEntityCommandBufferSystem.Singleton ecb;
            protected override void OnCreate()
            {
         
                 ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            }

            protected override void OnUpdate()
            {
                var depends = Dependency;
                depends = new GetAttackPosition()
                {
                    GetChild = SystemAPI.GetBufferLookup<Child>(),
                    Melee = SystemAPI.GetBufferLookup<MeleeAttackPosition>(),
                    Range = SystemAPI.GetBufferLookup<RangeAttackPosition>(),
                    testing = SystemAPI.GetBufferLookup<ReserveLocationTag>(false)
                    
                }.ScheduleParallel(depends);
                
                depends = new DetermineAction()
                {
                    deltaTime = SystemAPI.Time.DeltaTime,
                    ECB = ecb.CreateCommandBuffer(World.Unmanaged).AsParallelWriter(),
                }.Schedule(depends);
                Dependency = depends;
                Entities.WithoutBurst().WithStructuralChanges().ForEach(
                    (Entity entity, Command handler, Animator anim, NPCAttack comboList, in SelectAndAttack select) =>
                    {
                        handler.InputQueue ??= new Queue<AnimationTrigger>();
                        if (anim.IsInTransition(0)) return;
                        /*   handler.InputQueue.Enqueue(
                               comboList.AttackSequence.PickAttack(IAttackSequence.AttackType.Melee)[0]);
                          */
                        EntityManager.RemoveComponent<SelectAndAttack>(entity);
                        Debug.Log("attacked");
                    }).Run();
            }

            partial struct GetAttackPosition: IJobEntity
            {
                [ReadOnly] public BufferLookup<MeleeAttackPosition> Melee;
                [ReadOnly] public BufferLookup<RangeAttackPosition> Range;
                [ReadOnly] public BufferLookup<Child> GetChild;
                [NativeDisableParallelForRestriction]public BufferLookup<ReserveLocationTag> testing;

                void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, 
                    ref AttackState state, in AttackActionTag tag)
                {
                    var child = GetChild[state.TargetEntity][0].Value;
                    state.TargetPosition = float3.zero;

                    for (var i = 0; i < Melee[child].Length-1; i++)
                    {
                        var index = i;
                        var bufferElement = Melee[child][i];
                        if (bufferElement.State != OccupiedState.Vacant) continue;
                        state.TargetPosition = bufferElement.Position;
                        var buffer = testing[child];
                        buffer.Add(new ReserveLocationTag()
                        {
                            ReserverEntity = entity,
                            ID = index,
                        });
                        break;
                    }
                }

            }
            partial struct DetermineAction: IJobEntity
            {
                public float deltaTime;
                public EntityCommandBuffer.ParallelWriter ECB;
         
                void Execute([ChunkIndexInQuery]int chunkIndex, Entity entity, AttackAspect aspect,  in AttackActionTag tag)
                {

                   aspect.DeterminePlan();
                   aspect.ExecutePlan(entity, chunkIndex, deltaTime,ECB);
                }
            }

        }
    }
}