using System;
using System.Collections.Generic;
using System.Linq;
using AISenses;
using AISenses.VisionSystems;
using Dreamers.InventorySystem;
using DreamersInc.InfluenceMapSystem;
using Global.Component;
using IAUS.ECS.StateBlobSystem;
using ProjectDawn.Navigation;
using Stats.Entities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Component.Aspects
{
    public readonly partial struct IAUSBlackboard : IAspect
    {
        #region Components

        private readonly RefRO<LocalTransform> transform;
        readonly RefRO<AIStat> statInfo;
        private readonly RefRW<IAUSBrain> brain;
        private readonly RefRO<VisionIAUSLink> visionLink;
        private readonly RefRO<InfluenceComponent> influence;
        [Optional] private readonly RefRW<Patrol> patrol;
        [Optional] private readonly RefRW<Traverse> traverse;
        [Optional] private readonly RefRW<EvadeThreat> evade;
        [Optional] private readonly RefRW<TerrorizeAreaState> terrorizeArea;
        [Optional] private readonly RefRW<MaintenanceState> maintenance;
        [Optional] private readonly RefRO<ManualControlIAUS> manualControl;

        private string DebugText(AIStates state) =>
            $"Please check AI State Scriptable object and Consideration Data to make sure {state} state is implemented";

        private readonly Entity self;

        #endregion

        #region Derived Values

        private StateAsset GetAsset(int index)
        {
            return brain.ValueRO.State.Value.Array[index];
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

        private float DistanceToPoint(float3 posToCheck, float stopBuffer = 0.5f)
        {
            return Vector3.Distance(posToCheck, transform.ValueRO.Position) < stopBuffer
                ? 0
                : Vector3.Distance(posToCheck, transform.ValueRO.Position);
        }

        #endregion

        #region State Scores

        private float ScoreOfPatrolState
        {
            get
            {
                if (!patrol.IsValid) return 0.0f;
                if (patrol.ValueRO.Index == -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(patrol), DebugText(patrol.ValueRO.Name));
                }

                var asset = GetAsset(patrol.ValueRO.Index);
                patrol.ValueRW.DistanceToPoint =
                    DistanceToPoint(patrol.ValueRO.CurWaypoint.Position, patrol.ValueRO.BufferZone);
                float totalScore = asset.DistanceToTargetLocation.Output(patrol.ValueRO.DistanceRatio) *
                                   asset.Health.Output(statInfo.ValueRO
                                       .HealthRatio); //TODO Add Back Later * wander.ValueRO.TargetInRange.Output(attackRatio); ;
                patrol.ValueRW.TotalScore =
                    patrol.ValueRO.Status != ActionStatus.CoolDown && !patrol.ValueRO.AttackTarget
                        ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * patrol.ValueRO.mod) * totalScore)
                        : 0.0f;

                totalScore = patrol.ValueRW.TotalScore;
                return totalScore;
            }
        }

        private float ScoreOfTraverseState
        {
            get
            {
                if (!traverse.IsValid) return 0.0f;
                if (traverse.ValueRO.Index == -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(traverse), DebugText(traverse.ValueRO.Name));
                }

                traverse.ValueRW.DistanceToPoint =
                    DistanceToPoint(traverse.ValueRO.CurWaypoint.Position, traverse.ValueRO.BufferZone);
                var asset = GetAsset(traverse.ValueRO.Index);

                var totalScore = asset.DistanceToTargetLocation.Output(traverse.ValueRO.DistanceRatio) *
                                 asset.Health.Output(statInfo.ValueRO.HealthRatio);
                traverse.ValueRW.TotalScore = traverse.ValueRO.Status != ActionStatus.CoolDown
                    ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * traverse.ValueRO.mod) * totalScore)
                    : 0.0f;

                traverse.ValueRW.TotalScore = totalScore;
                return totalScore;
            }
        }

 
 

    
        private float ScoreOfEvadeState
        {
            get
            {
                if (!evade.IsValid) return 0.0f;

                if (!visionLink.ValueRO.TargetEnemyTargetInRange(out var dist))
                {
                    evade.ValueRW.EvadeTargetLocation = float3.zero;
                    evade.ValueRW.CheckInfluencePos = float3.zero;
                    return 0.0f;
                }

                evade.ValueRW.CheckInfluencePos = new float3(); // todo figure this out
                if (evade.ValueRO.Index == -1)
                    throw new ArgumentOutOfRangeException(nameof(evade), DebugText(evade.ValueRO.Name));
                var asset = GetAsset(evade.ValueRO.Index);
                var influenceRatio = Mathf.Clamp01(evade.ValueRO.InfluenceAtPoint.y /
                                                   (float)(influence.ValueRO.InfluenceValue +
                                                           evade.ValueRO.InfluenceAtPoint.x));
                var totalScore = asset.Health.Output(statInfo.ValueRO.HealthRatio)
                                 * asset.DistanceToTargetEnemy.Output(Mathf.Clamp01(dist / 200.0f))
                                 * asset.EnemyInfluence.Output(influenceRatio);
                evade.ValueRW.TotalScore = totalScore =
                    Mathf.Clamp01(totalScore + ((1.0f - totalScore) * evade.ValueRO.mod) * totalScore);

                return totalScore;
            }
        }

        private float ScoreOfTerrorizeArea
        {
            get
            {
                if (!terrorizeArea.IsValid) return 0.0f;
                if (terrorizeArea.ValueRO.TargetEntity == Entity.Null) return 0.0f;
                if (terrorizeArea.ValueRO.Index == -1)
                    throw new ArgumentOutOfRangeException(nameof(terrorizeArea), DebugText(terrorizeArea.ValueRO.Name));
                var asset = GetAsset(terrorizeArea.ValueRO.Index);

                var influenceRatio = Mathf.Clamp01(terrorizeArea.ValueRO.InfluenceAtTarget.y /
                                                   (float)(influence.ValueRO.InfluenceValue +
                                                           terrorizeArea.ValueRO.InfluenceAtTarget.x));
                var dist = !terrorizeArea.ValueRO.TargetPosition.Equals(float3.zero)
                    ? Vector3.Distance(transform.ValueRO.Position, terrorizeArea.ValueRO.TargetPosition)
                    : 300;

                var totalScore = asset.Health.Output(statInfo.ValueRO.HealthRatio) *
                                 asset.EnemyInfluence.Output(influenceRatio) *
                                 asset.DistanceToPlaceOfInterest.Output(Mathf.Clamp01(dist / 300.0f));
                terrorizeArea.ValueRW.TotalScore = totalScore =
                    Mathf.Clamp01(totalScore + ((1.0f - totalScore) * terrorizeArea.ValueRO.mod) * totalScore);

                return totalScore;
            }
        }

        private float ScoreOfMaintenanceState
        {
            get
            {
                if (!maintenance.IsValid || maintenance.ValueRO.MaintNeeded == MaintPlan.None) return 0.0f;

                if (maintenance.ValueRO.Index == -1)
                    throw new ArgumentOutOfRangeException(nameof(maintenance), DebugText(maintenance.ValueRO.Name));
                var asset = GetAsset(maintenance.ValueRO.Index);
                var influenceRatio = Mathf.Clamp01(maintenance.ValueRO.InfluenceAtPoint.y /
                                                   (float)(influence.ValueRO.InfluenceValue +
                                                           evade.ValueRO.InfluenceAtPoint.x));

                var dist = !maintenance.ValueRO.MaintLocation.Equals(float3.zero)
                    ? Vector3.Distance(transform.ValueRO.Position, maintenance.ValueRO.MaintLocation)
                    : 300;
                var totalScore = asset.Health.Output(statInfo.ValueRO.HealthRatio) *
                                 asset.EnemyInfluence.Output(influenceRatio)
                                 * asset.DistanceToPlaceOfInterest.Output(Mathf.Clamp01(dist / 300.0f));
                maintenance.ValueRW.TotalScore = totalScore =
                    Mathf.Clamp01(totalScore + ((1.0f - totalScore) * maintenance.ValueRO.mod) * totalScore);
                return totalScore;
            }
        }

        #endregion

        #region State Actions

        private AIStates GetHighState()
        {
            if (manualControl.IsValid)
            {
                return AIStates.None;
            }

            var stateInfo = new List<StateInfo>
            {
                new StateInfo(AIStates.Patrol,
                    patrol.IsValid ? patrol.ValueRO.Status : ActionStatus.Disabled, ScoreOfPatrolState),
                new StateInfo(AIStates.Traverse,
                    traverse.IsValid ? traverse.ValueRO.Status : ActionStatus.Disabled, ScoreOfTraverseState),
    

                new StateInfo(AIStates.Retreat, evade.IsValid ? evade.ValueRO.Status : ActionStatus.Disabled,
                    ScoreOfEvadeState),
                new StateInfo(AIStates.Terrorize,
                    terrorizeArea.IsValid ? terrorizeArea.ValueRO.Status : ActionStatus.Disabled, ScoreOfTerrorizeArea),
                new StateInfo(AIStates.PerformMaintenance,
                    maintenance.IsValid ? maintenance.ValueRO.Status : ActionStatus.Disabled, ScoreOfMaintenanceState)
            };

            var high = stateInfo.OrderByDescending(s => s.TotalScore)
                .FirstOrDefault(s => s.Status is ActionStatus.Idle or ActionStatus.Running);
            return high.TotalScore.Equals(0.0f) ? AIStates.None : high.StateName;
        }

        public void UpdateCurrentState(EntityCommandBuffer.ParallelWriter commandBufferParallel, int chunkIndex)
        {
            var highScoreState = GetHighState();
            if (brain.ValueRO.CurrentState == highScoreState) return;
            switch (brain.ValueRO.CurrentState)
            {
                case AIStates.Patrol:
                    commandBufferParallel.RemoveComponent<PatrolActionTag>(chunkIndex, self);
                    break;
                case AIStates.Traverse:
                    commandBufferParallel.RemoveComponent<TraverseActionTag>(chunkIndex, self);
                    break;

                case AIStates.RetreatToLocation:
                    commandBufferParallel.RemoveComponent<RetreatActionTag>(chunkIndex, self);
                    break;
                case AIStates.Retreat:
                    commandBufferParallel.RemoveComponent<RetreatActionTag>(chunkIndex, self);
                    break;
                case AIStates.Terrorize:
                    commandBufferParallel.RemoveComponent<TerrorizeAreaTag>(chunkIndex, self);
                    break;
                case AIStates.PerformMaintenance:
                    commandBufferParallel.RemoveComponent<MaintenanceTag>(chunkIndex, self);
                    break;
            }

            //add new action tag
            switch (highScoreState)
            {
                case AIStates.Patrol:
                    commandBufferParallel.AddComponent(chunkIndex, self,
                        new PatrolActionTag() { UpdateWayPoint = false });
                    break;
                case AIStates.Traverse:
                    commandBufferParallel.AddComponent(chunkIndex, self,
                        new TraverseActionTag() { UpdateWayPoint = false });
                    break;
                case AIStates.WanderQuadrant:
                    commandBufferParallel.AddComponent<WanderActionTag>(chunkIndex, self);
                    break;
             
                case AIStates.Attack:
                    commandBufferParallel.AddComponent<AttackActionTag>(chunkIndex, self);
                    commandBufferParallel.AddComponent<CheckAttackStatus>(chunkIndex, self);
                    break;
                case AIStates.Retreat:
                    commandBufferParallel.AddComponent<RetreatActionTag>(chunkIndex, self);
                    break;
                case AIStates.Terrorize:
                    commandBufferParallel.AddComponent<TerrorizeAreaTag>(chunkIndex, self);
                    commandBufferParallel.AddComponent<CheckAttackStatus>(chunkIndex, self);
                    break;
                case AIStates.PerformMaintenance:
                    commandBufferParallel.AddComponent<MaintenanceTag>(chunkIndex, self);
                    break;
            }

            brain.ValueRW.CurrentState = highScoreState;
        }

        #endregion
    }
}