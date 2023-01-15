//using Unity.Collections;
//using UnityEngine;
//using Utilities.ReactiveSystem;
//using Unity.Jobs;
//using IAUS.ECS.Component;
//using Unity.Entities;
//using Unity.Burst;
//using Global.Component;
//using Components.MovementSystem;
//using AISenses;

//[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<MoveToTargetActionTag, MoveToTarget, IAUS.ECS.Systems.Reactive.MoveToTargetReactor>.StateComponent))]
//[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<Vision, MoveToTarget, IAUS.ECS.Systems.Reactive.VisionReactor>.StateComponent))]

//[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MoveToTargetActionTag, MoveToTarget, IAUS.ECS.Systems.Reactive.MoveToTargetReactor>.ManageComponentAdditionJob))]
//[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<Vision, MoveToTarget, IAUS.ECS.Systems.Reactive.VisionReactor>.ManageComponentAdditionJob))]

//[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<MoveToTargetActionTag, MoveToTarget, IAUS.ECS.Systems.Reactive.MoveToTargetReactor>.ManageComponentRemovalJob))]
//[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<Vision, MoveToTarget, IAUS.ECS.Systems.Reactive.VisionReactor>.ManageComponentRemovalJob))]

//namespace IAUS.ECS.Systems.Reactive
//{
//    public struct MoveToTargetReactor : IComponentReactorTagsForAIStates<MoveToTargetActionTag, MoveToTarget>
//    {
//        public void ComponentAdded(Entity entity, ref MoveToTargetActionTag newComponent, ref MoveToTarget AIStateCompoment)
//        {
//            AIStateCompoment.Status = ActionStatus.Running;

//        }

//        public void ComponentRemoved(Entity entity, ref MoveToTarget AIStateCompoment, in MoveToTargetActionTag oldComponent)
//        {

//            if (AIStateCompoment.InRange)
//            {
//                AIStateCompoment.Status = ActionStatus.CoolDown;
//                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
//            }
//            else
//            {
//                AIStateCompoment.Status = ActionStatus.CoolDown;
//                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime * 2;

//            }
//        }

//        public void ComponentValueChanged(Entity entity, ref MoveToTargetActionTag newComponent, ref MoveToTarget AIStateCompoment, in MoveToTargetActionTag oldComponent)
//        {
//            Debug.Log("charnged");
//        }
//        public class MoveToReactive : AIReactiveSystemBase<MoveToTargetActionTag, MoveToTarget, MoveToTargetReactor>
//        {
//            protected override MoveToTargetReactor CreateComponentReactor()
//            {
//                return new MoveToTargetReactor();
//            }
//        }

//    }


//    public struct VisionReactor : IComponentReactorTagsForAIStates<Vision, MoveToTarget>
//    {
//        public void ComponentAdded(Entity entity, ref Vision newComponent, ref MoveToTarget AIStateCompoment)
//        {

//        }

//        public void ComponentRemoved(Entity entity, ref MoveToTarget AIStateCompoment, in Vision oldComponent)
//        {

//        }

//        public void ComponentValueChanged(Entity entity, ref Vision newComponent, ref MoveToTarget AIStateCompoment, in Vision oldComponent)
//        {

//        }

//        public class MoveToReactive : AIReactiveSystemBase<Vision, MoveToTarget, VisionReactor>
//        {
//            protected override VisionReactor CreateComponentReactor()
//            {
//                return new VisionReactor();
//            }
//        }

//    }

//    public class MoveToTargetReactive : SystemBase
//    {
//        private EntityQuery MoveToTargetUpdate;
//        private EntityQuery ActionTagAdded;

//        EntityCommandBufferSystem entityCommandBufferSystem;


//        protected override void OnCreate()
//        {
//            base.OnCreate();
//            MoveToTargetUpdate = GetEntityQuery(new EntityQueryDesc()
//            {
//                All = new ComponentType[] {  ComponentType.ReadWrite(typeof(MoveToTarget)), 
//                ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadWrite(typeof(ScanPositionBuffer)), ComponentType.ReadOnly(typeof(Vision))},

//            });
//            ActionTagAdded = GetEntityQuery(new EntityQueryDesc()
//            {
//                All = new ComponentType[] {  ComponentType.ReadWrite(typeof(MoveToTarget)), ComponentType.ReadOnly(typeof(MoveToTargetActionTag)),
//                ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadWrite(typeof(ScanPositionBuffer)), ComponentType.ReadOnly(typeof(Vision))},
//                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<MoveToTargetActionTag,MoveToTarget, MoveToTargetReactor>.StateComponent))}

//            });
//            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

//        }
//        protected override void OnUpdate()
//        {
//            JobHandle systemDeps = Dependency;

//            systemDeps = new UpdateMoveTarget()
//            {
//                MoveToChunk = GetComponentTypeHandle<MoveToTarget>(false),
//                SeersChunk = GetComponentTypeHandle<Vision>(true),
//                AItargetFromEntity = GetComponentDataFromEntity<AITarget>(false),
//                DT = Time.DeltaTime
//            }.ScheduleParallel(MoveToTargetUpdate, systemDeps);

//            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
//            systemDeps = new UpdateMoveTargetWhenActive()
//            {
//               MoveChunk = GetComponentTypeHandle<Movement>(false),
//                MoveToChunk = GetComponentTypeHandle<MoveToTarget>(false),
   
//            }.ScheduleParallel(ActionTagAdded, systemDeps);

//            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
//            Dependency = systemDeps;
//        }

//        public struct UpdateMoveTarget : IJobChunk

//        {
//            public ComponentTypeHandle<MoveToTarget> MoveToChunk;
//            [ReadOnly] public ComponentTypeHandle<Vision> SeersChunk;
//            [NativeDisableParallelForRestriction] public ComponentDataFromEntity<AITarget> AItargetFromEntity;
//            public float DT;
//            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//            {
//                NativeArray<MoveToTarget> moveToTargets = chunk.GetNativeArray(MoveToChunk);
//                NativeArray<Vision> Seers = chunk.GetNativeArray(SeersChunk);


//                for (int i = 0; i < chunk.Count; i++)
//                {

//                    if (Seers[i].ClosestTarget.target.entity==Entity.Null) {
//                        continue;
//                    }

//                    MoveToTarget moveTo = moveToTargets[i];
//                    if (!moveTo.CheckForTarget) 
//                    {
//                        moveTo.CheckTimer -= DT;
//                        moveToTargets[i] = moveTo;
//                        continue;
//                    }
//                    AITarget check = AItargetFromEntity[Seers[i].ClosestTarget.target.entity];

//                    if (!moveTo.HasTarget &&
//                        !Seers[i].ClosestTarget.Equals(new ScanPositionBuffer()) &&
//                        check.CanBeTargeted
//                        )
//                    {
//                        moveTo.Target = Seers[i].ClosestTarget;
//                        check.NumOfEntityTargetingMe++;
//                        AItargetFromEntity[moveTo.Target.target.entity] = check;

//                    }

//                    moveToTargets[i] = moveTo;
//                }
//            }
//        }

//        [BurstCompile]
//        public struct UpdateMoveTargetWhenActive : IJobChunk
//        {   
//            public ComponentTypeHandle<Movement> MoveChunk;
//            [ReadOnly] public ComponentTypeHandle<MoveToTarget> MoveToChunk;
//            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//            {
//                     NativeArray<Movement> movements = chunk.GetNativeArray(MoveChunk);
//                NativeArray<MoveToTarget> moveToTargets = chunk.GetNativeArray(MoveToChunk);

//                for (int i = 0; i < chunk.Count; i++)
//                {

//                    MoveToTarget moveTo = moveToTargets[i];
//                    Movement move = movements[i];

//                    if (moveTo.HasTarget )
//                    {
//                        move.TargetLocation = moveTo.Target.target.LastKnownPosition;
//                        move.CanMove = true;
//                        move.SetTargetLocation = true;
//                    }
//                    movements[i] = move;
//                }

//            }
//        }


//    }
//}