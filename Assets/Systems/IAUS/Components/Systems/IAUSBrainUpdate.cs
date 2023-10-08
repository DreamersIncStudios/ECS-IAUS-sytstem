using IAUS.ECS.Component;
using IAUS.ECS.Component.Aspects;
using IAUS.ECS.StateBlobSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace IAUS.ECS.Systems
{


    [UpdateAfter(typeof(SetupAIStateBlob))]
    public partial class IAUSUpdateGroup : ComponentSystemGroup
    {
        public IAUSUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(250, true);

        }

    }
    public partial class IAUSUpdateStateGroup : ComponentSystemGroup
    {
 
    }


    [UpdateInGroup(typeof(IAUSUpdateGroup))]
    public partial struct IAUSBrainUpdate : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }
        [BurstCompile]

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
  
            var patrolUpdate = new UpdatePatrol();
            patrolUpdate.Schedule();
         
            var traverseUpdate = new UpdateTraverse();
            traverseUpdate.Schedule();

            var WanderUpdate = new UpdateWander();
            WanderUpdate.Schedule();
            var waitUpdate = new UpdateWait();
            waitUpdate.Schedule();
            var attackUpdate = new UpdateAttack();
            attackUpdate.Schedule();
            new UpdateEscape().Schedule();

            new FindHighState() { CommandBufferParallel = ecb.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()}.Schedule();

        }
    }
    [BurstCompile]
    partial struct UpdatePatrol : IJobEntity {
        void Execute(PatrolAspect aspect, ref DynamicBuffer<StateBuffer> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].StateName == AIStates.Patrol)
                {
                    StateBuffer temp = buffer[i];
                    temp.TotalScore = aspect.Score;
                    temp.Status = aspect.Status;
                    buffer[i] = temp;

                    break;
                }
            }

        }

    }
    [BurstCompile]

    partial struct UpdateTraverse : IJobEntity
    {
        void Execute(TraverseAspect aspect, ref DynamicBuffer<StateBuffer> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].StateName == AIStates.Patrol)
                {
                    StateBuffer temp = buffer[i];
                    temp.TotalScore = aspect.Score;
                    temp.Status = aspect.Status;
                    buffer[i] = temp;

                    break;
                }
            }

        }

    }
    [BurstCompile]
    partial struct UpdateWander: IJobEntity
    {
        void Execute(WanderQuadrantAspect aspect, ref DynamicBuffer<StateBuffer> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].StateName == AIStates.WanderQuadrant)
                {
                    StateBuffer temp = buffer[i];
                    temp.TotalScore = aspect.Score;
                    temp.Status = aspect.Status;
                    buffer[i] = temp;

                    break;
                }
            }

        }

    }
    [BurstCompile]

    partial struct UpdateWait : IJobEntity
    {
        void Execute(WaitAspect aspect, ref DynamicBuffer<StateBuffer> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].StateName == AIStates.Wait)
                {
                    StateBuffer temp = buffer[i];
                    temp.TotalScore = aspect.Score;
                    temp.Status = aspect.Status;
                    buffer[i] = temp;

                    break;
                }
            }

        }

    }

    partial struct UpdateAttack : IJobEntity
    {
        void Execute(AttackAspect aspect, ref DynamicBuffer<StateBuffer> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].StateName == AIStates.Attack)
                {
                    StateBuffer temp = buffer[i];
                    temp.TotalScore = aspect.Score;
                    temp.Status = aspect.Status;
                    buffer[i] = temp;

                    break;
                }
            }

        }

    }
    [BurstCompile]

    partial struct UpdateEscape : IJobEntity
    {
        void Execute(EscapeAspect aspect, ref DynamicBuffer<StateBuffer> buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].StateName == AIStates.RetreatToLocation)
                {
                    StateBuffer temp = buffer[i];
                    temp.TotalScore = aspect.Score;
                    temp.Status = aspect.Status;
                    buffer[i] = temp;

                    break;
                }
            }

        }

    }

    [BurstCompile]

    partial struct FindHighState : IJobEntity {

        public EntityCommandBuffer.ParallelWriter CommandBufferParallel;

        void Execute([ChunkIndexInQuery] int chunkIndex,Entity entity, ref IAUSBrain brain, ref DynamicBuffer<StateBuffer> states) {
            StateBuffer tester = new();

            for (int j = 0; j < states.Length; j++)
            {
                if (tester.TotalScore < states[j].TotalScore && states[j].ConsiderScore)
                    tester = states[j];
            }
            if (tester.StateName != brain.CurrentState && tester.Status == ActionStatus.Idle)
            {
                switch (brain.CurrentState)
                {
                    case AIStates.Patrol:
                        CommandBufferParallel.RemoveComponent<PatrolActionTag>(chunkIndex, entity);
                        break;
                    case AIStates.Traverse:
                        CommandBufferParallel.RemoveComponent<TraverseActionTag>(chunkIndex, entity);
                        break;
                    case AIStates.Wait:
                        CommandBufferParallel.RemoveComponent<WaitActionTag>(chunkIndex, entity);
                        break;
                    case AIStates.WanderQuadrant:
                        CommandBufferParallel.RemoveComponent<WanderActionTag>(chunkIndex, entity);
                        break;
                    //case AIStates.ChaseMoveToTarget:
                    //    CommandBufferParallel.RemoveComponent<MoveToTargetActionTag>(chunkIndex, Entities[i]);
                    //    break;
                    //case AIStates.GotoLeader:
                    //    CommandBufferParallel.RemoveComponent<StayInRangeActionTag>(chunkIndex, Entities[i]);
                    //    break;
                    case AIStates.Attack:
                        //TODO Implement Add and Remove Tag;
                        CommandBufferParallel.RemoveComponent<AttackActionTag>(chunkIndex, entity);
                        break;

                    case AIStates.RetreatToLocation:
                        CommandBufferParallel.RemoveComponent<RetreatActionTag>(chunkIndex, entity);
                        break;
                        //case AIStates.GatherResources:
                        //    CommandBufferParallel.RemoveComponent<GatherResourcesTag>(chunkIndex, Entities[i]);
                        //    break;
                        //case AIStates.Heal_Magic:
                        //    CommandBufferParallel.RemoveComponent<HealSelfTag>(chunkIndex, Entities[i]);
                        //    break;
                        //case AIStates.CallBackUp:
                        //    CommandBufferParallel.RemoveComponent<SpawnTag>(chunkIndex, Entities[i]);
                        //    break;
                        //case AIStates.Terrorize:
                        //    CommandBufferParallel.RemoveComponent<TerrorizeAreaTag>(chunkIndex, Entities[i]);
                        //    break;
                }
                //add new action tag
                switch (tester.StateName)
                {
                    case AIStates.Patrol:
                        CommandBufferParallel.AddComponent(chunkIndex, entity, new PatrolActionTag() { UpdateWayPoint = false });
                        break;
                    case AIStates.Traverse:
                        CommandBufferParallel.AddComponent(chunkIndex, entity, new TraverseActionTag() { UpdateWayPoint = false });
                        break;
                    case AIStates.WanderQuadrant:
                        CommandBufferParallel.AddComponent<WanderActionTag>(chunkIndex, entity);
                        break;
                    case AIStates.Wait:
                        CommandBufferParallel.AddComponent<WaitActionTag>(chunkIndex, entity);
                        break;
                    //case AIStates.ChaseMoveToTarget:
                    //    CommandBufferParallel.AddComponent<MoveToTargetActionTag>(chunkIndex, Entities[i]);
                    //    break;
                    //case AIStates.GotoLeader:
                    //    CommandBufferParallel.AddComponent<StayInRangeActionTag>(chunkIndex, Entities[i]);
                    //    break;
                    case AIStates.Attack:
                        CommandBufferParallel.AddComponent<AttackActionTag>(chunkIndex, entity);
                        break;
                    case AIStates.RetreatToLocation:
                        CommandBufferParallel.AddComponent<RetreatActionTag>(chunkIndex, entity);
                        break;
                        //case AIStates.GatherResources:
                        //    CommandBufferParallel.AddComponent<GatherResourcesTag>(chunkIndex, Entities[i]);
                        //    break;
                        //case AIStates.Heal_Magic:
                        //    CommandBufferParallel.AddComponent<HealSelfTag>(chunkIndex, Entities[i]);
                        //    break;
                        //case AIStates.CallBackUp:
                        //    CommandBufferParallel.AddComponent<SpawnTag>(chunkIndex, Entities[i]);
                        //    break;
                        //case AIStates.Terrorize:
                        //    CommandBufferParallel.AddComponent<TerrorizeAreaTag>(chunkIndex, Entities[i]);
                        //    break;
                }
                brain.CurrentState = tester.StateName;
            }
          


        }
    }
}