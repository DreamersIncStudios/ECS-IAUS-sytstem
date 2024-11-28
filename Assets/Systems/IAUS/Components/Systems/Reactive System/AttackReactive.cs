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
using UnityEngine.SocialPlatforms;
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
            aiStateComponent.TargetPosition = float3.zero;
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
                
                depends = new DetermineAction()
                {
                    deltaTime = SystemAPI.Time.DeltaTime,
                    ECB = ecb.CreateCommandBuffer(World.Unmanaged).AsParallelWriter(),
                }.Schedule(depends);
                depends = new GetAttackPosition()
                {
                    ChildBufferLookup = SystemAPI.GetBufferLookup<Child>(),
                    MeleeAttackPositions = SystemAPI.GetBufferLookup<MeleeAttackPosition>(),
                    RangedAttackBuffer = SystemAPI.GetBufferLookup<RangeAttackPosition>(),
                    ReserveLocationBuffer = SystemAPI.GetBufferLookup<ReserveLocationTag>(false)
                }.Schedule(depends);
                depends = new CheckAttackPosition()
                {
                    ChildBufferLookup = SystemAPI.GetBufferLookup<Child>(),
                    MeleeBufferLookup = SystemAPI.GetBufferLookup<MeleeAttackPosition>(),
                    LocalTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>()
                }.Schedule(depends);
                Dependency = depends;
                Entities.WithoutBurst().WithStructuralChanges().ForEach(
                    (Entity entity, Command handler, Animator anim, NPCAttack comboList, in SelectAndAttack select) =>
                    {
                        handler.InputQueue ??= new Queue<AnimationTrigger>();
                        if (anim.IsInTransition(0)) return;
                        /*   handler.InputQueue.Enqueue(
                               comboList.AttackSequence.PickAttack(IAttackSequence.AttackType.MeleeAttackPositions)[0]);
                          */
                        EntityManager.RemoveComponent<SelectAndAttack>(entity);
                        Debug.Log("attacked");
                    }).Run();
            }

            partial struct GetAttackPosition: IJobEntity
            {
                [ReadOnly] public BufferLookup<MeleeAttackPosition> MeleeAttackPositions;
                [ReadOnly] public BufferLookup<RangeAttackPosition> RangedAttackBuffer;
                [ReadOnly] public BufferLookup<Child> ChildBufferLookup;
                [NativeDisableParallelForRestriction]public BufferLookup<ReserveLocationTag> ReserveLocationBuffer;

                void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, 
                    ref AttackState state, in AttackActionTag tag)
                {
                    if(state.AttackPlans.IsEmpty) return;
                    if (state.AttackPlans[0] != AttackPlan.GetAttackLocation)
                        return;

                    var child = ChildBufferLookup[state.TargetEntity][0].Value;
                    state.TargetPosition = float3.zero;

                    for (var i = 0; i < MeleeAttackPositions[child].Length-1; i++)
                    {
                        var index = i;
                        var bufferElement = MeleeAttackPositions[child][i];
                        if (bufferElement.State != OccupiedState.Vacant) continue;
                        state.TargetPosition = bufferElement.Position;
                        var buffer = ReserveLocationBuffer[child];
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

            private partial struct CheckAttackPosition:IJobEntity
            {
                public ComponentLookup<LocalTransform> LocalTransformLookup;
                public BufferLookup<Child> ChildBufferLookup;
                public BufferLookup<MeleeAttackPosition> MeleeBufferLookup;
                void Execute(ref AttackState state, in AttackActionTag tag)
                {
                    var dist = Vector3.Distance(state.TargetPosition,LocalTransformLookup[state.TargetEntity].Position);
                    if (dist < 10) return; 
                    var child = ChildBufferLookup[state.TargetEntity][0].Value;
                    state.TargetPosition = MeleeBufferLookup[child][state.TargetPosistionID];
                    if(state.AttackPlans[0] != AttackPlan.MoveToLocationMelee)
                        state.AttackPlans.Insert(0, AttackPlan.MoveToLocationMelee);
                }
            }
        }
    }
}