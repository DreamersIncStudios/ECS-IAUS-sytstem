using System;
using System.Collections.Generic;
using System.Linq;
using AISenses.VisionSystems;
using DreamersIncStudio.GAIACollective;
using IAUS.ECS.Component;
using IAUS.ECS.Component.Aspects;
using IAUS.ECS.StateBlobSystem;
using Stats.Entities;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace IAUS.ECS.Systems
{
    [UpdateAfter(typeof(SetupAIStateBlob))]
    [UpdateAfter(typeof(GaiaUpdateGroup))]
    public partial class IAUSUpdateGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<RunningTag>();
        }

        public IAUSUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(1000, true);

        }
    }
    public partial class IAUSUpdateStateGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<RunningTag>();
        }

    }

    [UpdateInGroup(typeof(IAUSUpdateGroup))]
    public partial struct IAUSBrainUpdate : ISystem
    {
        
        [BurstCompile]

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            new IAUSUpdateJob()
                    { CommandBufferParallel = ecb.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() }
                .ScheduleParallel();

        }
    }
    public partial struct IAUSUpdateJob: IJobEntity
    {
        private string DebugText(AIStates state) =>
            $"Please check AI State Scriptable object and Consideration Data to make sure {state} state is implemented";
        
        public EntityCommandBuffer.ParallelWriter CommandBufferParallel;
       
        void Execute(Entity self, [ChunkIndexInQuery] int chunkIndex, DynamicBuffer<StateData> StatesToCheck, ref IAUSBrain brain, ref VisionIAUSLink visionLink, ref AIStat statInfo)
        {
            var stateInfo = new List<StateInfo>();
            foreach (var stateData in StatesToCheck)
            {
                var score = 0.0f;
                var mod = 0.0f;
                var totalScore = 0.0f;
                StateAsset asset = new StateAsset();
                switch (stateData.State)
                {

                    case AIStates.None:
                        break;
                    case AIStates.Patrol:
                        break;
                    case AIStates.Heal_Self_Item:
                        break;
                    case AIStates.Heal_Magic:
                        break;
                    case AIStates.Attack:
                        if (!visionLink.TargetEnemyTargetInRange(out float distAttackTarget))
                            if (stateData.Index == -1)
                            {
                                throw new ArgumentOutOfRangeException(nameof(stateData.State), DebugText(stateData.State));
                            }
                        asset = brain.State.Value.Array[stateData.Index];
                        var influenceDist = 0; //Todo Figure this out 
                        var totalScoreAttack = asset.Health.Output(statInfo.HealthRatio) *
                                         asset.DistanceToTargetEnemy.Output(distAttackTarget / 200.0f) *
                                         asset.EnemyInfluence.Output(influenceDist);
                         mod =1.0f - (1.0f / 4.0f);  
                        score = Mathf.Clamp01(totalScoreAttack + ((1.0f - totalScoreAttack) * mod) * totalScoreAttack);
                        
                        break;
                    case AIStates.Retreat:
                        break;
                    case AIStates.FindCover:
                        break;
                    case AIStates.Talk:
                        break;
                    case AIStates.Guard:
                        break;
                    case AIStates.GroupUp:
                        break;
                    case AIStates.GotoLeader:
                        break;
                    case AIStates.InvestigateArea:
                        break;
                    case AIStates.SearchArea:
                        break;
                    case AIStates.RetreatToLocation:
                        break;
                    case AIStates.RetreatToQuadrant:
                        break;
                    case AIStates.FollowTarget:
                        break;
                    case AIStates.ChaseMoveToTarget:
                        break;
                    case AIStates.Traverse:
                        break;
                    case AIStates.GatherResources:
                        break;
                    case AIStates.SpawnPackHerd:
                        break;
                    case AIStates.Terrorize:
                        break;
                    case AIStates.WanderQuadrant:

                        if (stateData.Index == -1)
                        {
                            throw new ArgumentOutOfRangeException(nameof(stateData.State), DebugText(stateData.State));
                        }

                        asset = brain.State.Value.Array[stateData.Index];
                   

                        var distToEnemy = visionLink.TargetEnemyTargetInRange(out float dist)
                            ? dist
                            : 200.0f;
                         totalScore = Mathf.Clamp01(asset.Health.Output(statInfo.HealthRatio) *
                                                    asset.DistanceToTargetEnemy.Output(
                                                        Mathf.Clamp01(distToEnemy /
                                                                      200.0f))); //TODO Add Back Later * escape.ValueRO.TargetInRange.Output(attackRatio); ;
              
                      
                        mod =1.0f - (1.0f / 2.0f);  
                        score = Mathf.Clamp01(totalScore + ((1.0f - totalScore) * mod) * totalScore);
                        break;
                    case AIStates.PerformMaintenance:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                stateInfo.Add(new StateInfo(stateData.State, stateData.Status, score));
            }
            
            var high = stateInfo.OrderByDescending(s => s.TotalScore)
                .FirstOrDefault(s => s.Status is ActionStatus.Idle or ActionStatus.Running);
            
            if(brain.CurrentState == high.StateName) return;
            
            switch (brain.CurrentState)
            {
                case AIStates.Patrol:
                    CommandBufferParallel.RemoveComponent<PatrolActionTag>(chunkIndex, self);
                    break;
                case AIStates.Traverse:
                    CommandBufferParallel.RemoveComponent<TraverseActionTag>(chunkIndex, self);
                    break;
           
                case AIStates.WanderQuadrant:
                    CommandBufferParallel.RemoveComponent<WanderActionTag>(chunkIndex, self);
                    break;
                case AIStates.Attack:
                    CommandBufferParallel.RemoveComponent<AttackActionTag>(chunkIndex, self);
                    break;

                case AIStates.RetreatToLocation:
                    CommandBufferParallel.RemoveComponent<RetreatActionTag>(chunkIndex, self);
                    break;
                case AIStates.Retreat:
                    CommandBufferParallel.RemoveComponent<RetreatActionTag>(chunkIndex, self);
                    break;
                case AIStates.Terrorize:
                    CommandBufferParallel.RemoveComponent<TerrorizeAreaTag>(chunkIndex, self);
                    break;
                case AIStates.PerformMaintenance:
                    CommandBufferParallel.RemoveComponent<MaintenanceTag>(chunkIndex, self);
                    break;
            }
            switch (high.StateName)
            {          case AIStates.Patrol:
                    CommandBufferParallel.AddComponent(chunkIndex, self,
                        new PatrolActionTag() { UpdateWayPoint = false });
                    break;
                case AIStates.Traverse:
                    CommandBufferParallel.AddComponent(chunkIndex, self,
                        new TraverseActionTag() { UpdateWayPoint = false });
                    break;
                case AIStates.WanderQuadrant:
                    CommandBufferParallel.AddComponent<WanderActionTag>(chunkIndex, self);
                    break;
        
                case AIStates.Attack:
                    CommandBufferParallel.AddComponent<AttackActionTag>(chunkIndex, self);
                    break;
                case AIStates.Retreat:
                    CommandBufferParallel.AddComponent<RetreatActionTag>(chunkIndex, self);
                    break;
                case AIStates.Terrorize:
                    CommandBufferParallel.AddComponent<TerrorizeAreaTag>(chunkIndex, self);
                    break;
                case AIStates.PerformMaintenance:
                    CommandBufferParallel.AddComponent<MaintenanceTag>(chunkIndex, self);
                    break;
            }
            brain.CurrentState = high.StateName;
        }
        private struct StateInfo
        {
            public AIStates StateName { get; private set; }
            public readonly float TotalScore;
            public readonly ActionStatus Status;

            public StateInfo(AIStates state, ActionStatus status, float score)
            {
                StateName = state;
                Status = status;
                TotalScore = score;
            }
        }
    }
    
}