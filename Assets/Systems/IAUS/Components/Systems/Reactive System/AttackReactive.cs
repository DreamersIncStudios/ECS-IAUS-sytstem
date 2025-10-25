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


[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBuffer<AttackActionTag, StateData, IAUS.ECS.Systems.Reactive.AttackTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBuffer<AttackActionTag, StateData, IAUS.ECS.Systems.Reactive.AttackTagReactor>.ManageComponentAdditionJob))]

namespace IAUS.ECS.Systems.Reactive
{

    public partial struct AttackTagReactor : IComponentReactorTagsForAIBuffer<AttackActionTag, StateData>
    {
        public void ComponentAdded(Entity entity, ref AttackActionTag newAITag, DynamicBuffer<StateData> AIStateCompoment)
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

        public void ComponentRemoved(Entity entity, DynamicBuffer<StateData>  AIStateCompoment, in AttackActionTag oldComponent)
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
        }

  

        public partial class ReactiveSystem : AIReactiveSystemBuffer<AttackActionTag, StateData, AttackTagReactor>
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
                
                depends = new DetermineAttackAction()
                {
                    DeltaTime = SystemAPI.Time.DeltaTime,
                    ECB = ecb.CreateCommandBuffer(World.Unmanaged).AsParallelWriter(),
                }.Schedule(depends);
                
                depends = new ExecuteAttackAction()
                {
                    DeltaTime = SystemAPI.Time.DeltaTime,
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

                void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref LocalTransform transform, 
                    ref AttackActionTag state)
                {
                    if(state.AttackPlans.IsEmpty) return;
                    if (state.AttackPlans[0] != AttackPlan.GetAttackLocation)
                        return;

                    var child = ChildBufferLookup[state.TargetEntity][0].Value;
                    state.TargetPosition = float3.zero;

                    List<DistCheck> dist = new();
                    var buffer = MeleeAttackPositions[child];
                    for (var i = 0; i < buffer.Length-1; i++)
                    {
                        var index = i; ;
                        dist.Add(
                            new DistCheck()
                            {
                                Distance =
                                    Vector3.Distance(transform.Position, buffer[i].Position),
                                Index =  index
                            });
                    }

                    var orderBy = dist.OrderBy(x =>x.Distance);

                    foreach (var check in orderBy)
                    {
                        if (buffer[check.Index].State != OccupiedState.Vacant) continue;
                        ReserveLocationBuffer[child].Add(new ReserveLocationTag()
                        {
                            ReserverEntity = entity,
                            ID = check.Index
                        });
                        break;
                    }
                }

                class DistCheck
                {
                    public int Index;
                    public float Distance;
                }
            }
            

            private partial struct CheckAttackPosition:IJobEntity
            {
                public ComponentLookup<LocalTransform> LocalTransformLookup;
                public BufferLookup<Child> ChildBufferLookup;
                public BufferLookup<MeleeAttackPosition> MeleeBufferLookup;
                void Execute(ref AttackActionTag state)
                {
                    if (state.TargetPositionID == -1) return;
                    if (state.TargetPositionID > 4)
                    {
                        state.TargetPositionID = -1;
                        state.AttackPlans.Insert(0, AttackPlan.GetAttackLocation);
                        return;
                    }

                    var dist = Vector3.Distance(state.TargetPosition,LocalTransformLookup[state.TargetEntity].Position);
                    if (dist < 10) return; 
                    var child = ChildBufferLookup[state.TargetEntity][0].Value;
                    state.TargetPosition = MeleeBufferLookup[child][state.TargetPositionID];
                    if(state.AttackPlans[0] != AttackPlan.MoveToLocationMelee)
                        state.AttackPlans.Insert(0, AttackPlan.MoveToLocationMelee);
                }
            }
        }
    }
}