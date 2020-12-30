using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS2.Component;
using Unity.Entities;
using Unity.Burst;
using Global.Component;
using Components.MovementSystem;
using AISenses;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<MoveToTargetActionTag, MoveToTarget, IAUS.ECS2.Systems.Reactive.MoveToTargetReactor>.StateComponent))]
[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<Vision, MoveToTarget, IAUS.ECS2.Systems.Reactive.VisionReactor>.StateComponent))]
namespace IAUS.ECS2.Systems.Reactive
{
    public struct MoveToTargetReactor : IComponentReactorTagsForAIStates<MoveToTargetActionTag, MoveToTarget>
    {
        public void ComponentAdded(Entity entity, ref MoveToTargetActionTag newComponent, ref MoveToTarget AIStateCompoment)
        {
        }

        public void ComponentRemoved(Entity entity, ref MoveToTarget AIStateCompoment, in MoveToTargetActionTag oldComponent)
        {
        }

        public void ComponentValueChanged(Entity entity, ref MoveToTargetActionTag newComponent, ref MoveToTarget AIStateCompoment, in MoveToTargetActionTag oldComponent)
        {
        }
        public class MoveToReactive : AIReactiveSystemBase<MoveToTargetActionTag, MoveToTarget, MoveToTargetReactor>
        {
            protected override MoveToTargetReactor CreateComponentReactor()
            {
                return new MoveToTargetReactor();
            }
        }

    }


    public struct VisionReactor : IComponentReactorTagsForAIStates<Vision, MoveToTarget>
    {
        public void ComponentAdded(Entity entity, ref Vision newComponent, ref MoveToTarget AIStateCompoment)
        {

        }

        public void ComponentRemoved(Entity entity, ref MoveToTarget AIStateCompoment, in Vision oldComponent)
        {

        }

        public void ComponentValueChanged(Entity entity, ref Vision newComponent, ref MoveToTarget AIStateCompoment, in Vision oldComponent)
        {

        }

        public class MoveToReactive : AIReactiveSystemBase<Vision, MoveToTarget, VisionReactor>
        {
            protected override VisionReactor CreateComponentReactor()
            {
                return new VisionReactor();
            }
        }

    }

    public class MoveToTargetReactive : SystemBase
    {
        private EntityQuery MoveToTargetUpdate;
        private EntityQuery ActionTagAdded;

        EntityCommandBufferSystem entityCommandBufferSystem;


        protected override void OnCreate()
        {
            base.OnCreate();
            MoveToTargetUpdate = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] {  ComponentType.ReadWrite(typeof(MoveToTarget)), 
                ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadWrite(typeof(ScanPositionBuffer)), ComponentType.ReadOnly(typeof(Vision))},

            });
            ActionTagAdded = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] {  ComponentType.ReadWrite(typeof(MoveToTarget)), ComponentType.ReadOnly(typeof(MoveToTargetActionTag)),
                ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadWrite(typeof(ScanPositionBuffer)), ComponentType.ReadOnly(typeof(Vision))},
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<MoveToTargetActionTag,MoveToTarget, MoveToTargetReactor>.StateComponent))}

            });
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new UpdateMoveTarget()
            {
                MoveToChunk = GetArchetypeChunkComponentType<MoveToTarget>(false),
                SeersChunk = GetArchetypeChunkComponentType<Vision>(true),
                AItargetFromEntity = GetComponentDataFromEntity<AITarget>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(MoveToTargetUpdate, systemDeps);

            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new UpdateMoveTargetWhenActive()
            {
               MoveChunk = GetArchetypeChunkComponentType<Movement>(false),
                MoveToChunk = GetArchetypeChunkComponentType<MoveToTarget>(false),
   
            }.ScheduleParallel(ActionTagAdded, systemDeps);

            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;
        }

        public struct UpdateMoveTarget : IJobChunk

        {
        //    public ArchetypeChunkComponentType<Movement> MoveChunk;
            public ArchetypeChunkComponentType<MoveToTarget> MoveToChunk;
            [ReadOnly] public ArchetypeChunkComponentType<Vision> SeersChunk;
            [NativeDisableParallelForRestriction] public ComponentDataFromEntity<AITarget> AItargetFromEntity;
            public float DT;
            //   public ArchetypeChunkComponentType<AIReactiveSystemBase<Vision, MoveToTarget, VisionReactor>.StateComponent> StateChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
           //     NativeArray<Movement> movements = chunk.GetNativeArray(MoveChunk);
                NativeArray<MoveToTarget> moveToTargets = chunk.GetNativeArray(MoveToChunk);
                NativeArray<Vision> Seers = chunk.GetNativeArray(SeersChunk);
                //NativeArray<AIReactiveSystemBase<Vision, MoveToTarget, VisionReactor>.StateComponent> stateComponents = chunk.GetNativeArray(StateChunk);


                for (int i = 0; i < chunk.Count; i++)
                {
         


                    MoveToTarget moveTo = moveToTargets[i];
                    //        Movement move = movements[i];
                    if (!moveTo.CheckForTarget) 
                    {
                        moveTo.CheckTimer -= DT;
                        moveToTargets[i] = moveTo;
                        continue;
                    }
                    AITarget check = AItargetFromEntity[Seers[i].ClosestTarget.target.entity];

                    if (!moveTo.HasTarget &&
                        !Seers[i].ClosestTarget.Equals(new ScanPositionBuffer()) &&
                        check.CanBeTargeted
                        )
                    {
                        moveTo.Target = Seers[i].ClosestTarget;
                        check.NumOfEntityTargetingMe++;
                        AItargetFromEntity[moveTo.Target.target.entity] = check;
                    //    move.TargetLocation = moveTo.Target.target.PositionSawAt;
                    //    move.CanMove = true;
                    //    move.SetTargetLocation = true;
                    }
                    //movements[i] = move;

                    moveToTargets[i] = moveTo;
                }
            }
        }

        [BurstCompile]
        public struct UpdateMoveTargetWhenActive : IJobChunk
        {   
            public ArchetypeChunkComponentType<Movement> MoveChunk;
            [ReadOnly] public ArchetypeChunkComponentType<MoveToTarget> MoveToChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                     NativeArray<Movement> movements = chunk.GetNativeArray(MoveChunk);
                NativeArray<MoveToTarget> moveToTargets = chunk.GetNativeArray(MoveToChunk);

                for (int i = 0; i < chunk.Count; i++)
                {

                    MoveToTarget moveTo = moveToTargets[i];
                    Movement move = movements[i];

                    if (moveTo.HasTarget )
                    {
                        move.TargetLocation = moveTo.Target.target.PositionSawAt;
                        move.CanMove = true;
                        move.SetTargetLocation = true;
                    }
                    movements[i] = move;
                }

            }
        }


    }
}