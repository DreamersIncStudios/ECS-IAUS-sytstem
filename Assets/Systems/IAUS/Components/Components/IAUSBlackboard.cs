using System;
using System.Collections.Generic;
using System.Linq;
using AISenses;
using AISenses.VisionSystems;
using DreamersInc.InflunceMapSystem;
using Global.Component;
using IAUS.ECS.StateBlobSystem;
using Stats.Entities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IAUS.ECS.Component.Aspects
{


    public readonly partial struct IAUSBlackboard : IAspect
    {
        public readonly RefRO<LocalTransform> Transform;
        readonly RefRO<AIStat> statInfo;
        private readonly RefRW<IAUSBrain> brain;
        private readonly VisionAspect visionAspect;
        private readonly InfluenceAspect influenceAspect;
        [Optional] private readonly RefRW<Patrol> patrol;
        [Optional] private readonly RefRW<Traverse> traverse;
        [Optional] private readonly RefRW<WanderQuadrant> wander;
        [Optional] private readonly RefRW<Wait> wait;
        [Optional] private readonly RefRW<AttackState> attack;
        [Optional] private readonly RefRW<EvadeThreat> evade;

    public readonly Entity Self;

        private StateAsset GetAsset(int index)
        {
            return brain.ValueRO.State.Value.Array[index];
        }

        public struct StateInfo
        {
            [SerializeField] public AIStates StateName { get; private set; }
            public float TotalScore;
            public ActionStatus Status;
            public bool ConsiderScore => Status == ActionStatus.Idle || Status == ActionStatus.Running;

            public StateInfo(AIStates state, ActionStatus status, float score)
            {
                StateName = state;
                Status = status;
                TotalScore = score;
            }

        }

        float DistanceToPoint(float3 posToCheck, float StopBuffer = 0.5f)
        {
            return Vector3.Distance(posToCheck, Transform.ValueRO.Position) < StopBuffer
                ? 0
                : Vector3.Distance(posToCheck, Transform.ValueRO.Position);
        }

        public float ScoreOfPatrolState
        {
            get
            {
                if (!patrol.IsValid) return 0.0f;
                if (patrol.ValueRO.Index == -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(patrol),
                        $"Please check Creature list and Consideration Data to make sure {patrol.ValueRO.Name} state is implements");

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

        public float ScoreOfTraverseState
        {
            get
            {
                if (!traverse.IsValid) return 0.0f;
                if (traverse.ValueRO.Index == -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(traverse),
                        $"Please check Creature list and Consideration Data to make sure {traverse.ValueRO.Name} state is implements");
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

        float ScoreOfWanderState
        {
            get
            {
                if (!wander.IsValid) return 0.0f;

                if (wander.ValueRO.Index == -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(wander),
                        $"Please check Creature list and Consideration Data to make sure {wander.ValueRO.Name} state is implements");
                }

                if (wander.ValueRO.Status == ActionStatus.Idle &&
                    wander.ValueRO.SpawnPosition.Equals(wander.ValueRO.TravelPosition))
                {
                    wander.ValueRW.SpawnPosition.x += 35;
                    wander.ValueRW.SpawnPosition.z += 45;
                    wander.ValueRW.StartingDistance =
                        DistanceToPoint(wander.ValueRO.TravelPosition, wander.ValueRO.BufferZone);
                }

                var asset = GetAsset(wander.ValueRO.Index);
                wander.ValueRW.DistanceToPoint =
                    DistanceToPoint(wander.ValueRO.TravelPosition, wander.ValueRO.BufferZone);
                ;


                var distToEnemy = visionAspect.TargetEnemyTargetInRange(out float dist)
                    ? dist : 200.0f;
                var totalScore = Mathf.Clamp01(asset.DistanceToTargetLocation.Output(wander.ValueRO.DistanceRatio) *
                                               asset.Health.Output(statInfo.ValueRO.HealthRatio) *
                                               asset.DistanceToTargetEnemy.Output(
                                                   Mathf.Clamp01(distToEnemy /
                                                                 200.0f))); //TODO Add Back Later * escape.ValueRO.TargetInRange.Output(attackRatio); ;
                wander.ValueRW.TotalScore =
                    wander.ValueRO.Status != ActionStatus.CoolDown && !wander.ValueRO.AttackTarget
                        ? Mathf.Clamp01(totalScore + ((1.0f - totalScore) * wander.ValueRO.mod) * totalScore)
                        : 0.0f;

                totalScore = wander.ValueRW.TotalScore;
                return totalScore;
            }
        }

        float ScoreOfWaitState
        {
            get
            {
                if (!wait.IsValid) return 0.0f;
                if (wait.ValueRO.Index == -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(wait),
                        $"Please check Creature list and Consideration Data to make sure {wait.ValueRO.Name} state is implements");

                }

                var asset = GetAsset(wait.ValueRO.Index);
                float TotalScore = asset.Timer.Output(wait.ValueRO.TimePercent) *
                                   asset.Health.Output(statInfo.ValueRO.HealthRatio);
                wait.ValueRW.TotalScore =
                    Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * wait.ValueRO.mod) * TotalScore);
                TotalScore = wait.ValueRW.TotalScore;
                return TotalScore;
            }

        }

        private float ScoreOfAttackState
        {
            get
            {
                if (!attack.IsValid) return 0.0f;
                if (!visionAspect.TargetEnemyTargetInRange(out float dist))
                {
                    attack.ValueRW.TargetPosition = float3.zero;
                    return 0.0f;
                }

                attack.ValueRW.TargetPosition = visionAspect.TargetEnemyPosition;
            
                if (wait.ValueRO.Index == -1)
                {
                    throw new ArgumentOutOfRangeException(nameof(wait),
                        $"Please check Creature list and Consideration Data to make sure {wait.ValueRO.Name} state is implements");

                }
                var asset = GetAsset(attack.ValueRO.Index);
                var influenceDist = Mathf.Clamp01(influenceAspect.DistanceToHighProtection / TravelInFiveSec);
                float totalScore = asset.Health.Output(statInfo.ValueRO.HealthRatio) *
                                   asset.DistanceToTargetEnemy.Output(dist / 200.0f) *
                                   asset.EnemyInfluence.Output(influenceDist);
                totalScore = Mathf.Clamp01(totalScore + ((1.0f - totalScore) * attack.ValueRO.mod) * totalScore);
                attack.ValueRW.TotalScore = totalScore;
                return totalScore;
            }

        }

        private float ScoreOfEvadeState
        {
            get
            {
                if(evade.ValueRO.Index==-1)
                    throw new ArgumentOutOfRangeException(nameof(wait),
                        $"Please check Creature list and Consideration Data to make sure {evade.ValueRO.Name} state is implements");
                var asset = GetAsset(evade.ValueRO.Index);

                var totalScore = asset.Health.Output(statInfo.ValueRO.HealthRatio);
                evade.ValueRW.TotalScore = totalScore =
                    Mathf.Clamp01(totalScore + ((1.0f - totalScore) * attack.ValueRO.mod) * totalScore);
                return totalScore;
            }
        }
        private float TravelInFiveSec => statInfo.ValueRO.Speed * 5;

        private AIStates GetHighState()
        {
            var stateInfo = new List<StateInfo>
            {
                new StateInfo(AIStates.Attack, attack.IsValid ? attack.ValueRO.Status : ActionStatus.Disabled,
                    ScoreOfAttackState),
                new StateInfo(AIStates.Patrol,
                    patrol.IsValid ? patrol.ValueRO.Status : ActionStatus.Disabled, ScoreOfPatrolState),
                new StateInfo(AIStates.Traverse,
                    traverse.IsValid ? traverse.ValueRO.Status : ActionStatus.Disabled, ScoreOfTraverseState),
                new StateInfo(AIStates.WanderQuadrant,
                    wander.IsValid ? wander.ValueRO.Status : ActionStatus.Disabled, ScoreOfWanderState),
                new StateInfo(AIStates.Wait,
                    wait.IsValid ? wait.ValueRO.Status : ActionStatus.Disabled, ScoreOfWaitState),
                new StateInfo( AIStates.Retreat, evade.IsValid? evade.ValueRO.Status: ActionStatus.Disabled,ScoreOfEvadeState)
            };

            var high = stateInfo.OrderByDescending(s => s.TotalScore)
                .FirstOrDefault(s => s.Status is ActionStatus.Idle or ActionStatus.Running);
            return high.TotalScore == 0.0f ? AIStates.None : high.StateName;
        }

        public void UpdateCurrentState(EntityCommandBuffer.ParallelWriter commandBufferParallel, int chunkIndex)
        {

            var highScoreState = GetHighState();
            if (brain.ValueRO.CurrentState == highScoreState) return;
            switch (brain.ValueRO.CurrentState)
            {
                case AIStates.Patrol:
                    commandBufferParallel.RemoveComponent<PatrolActionTag>(chunkIndex, Self);
                    break;
                case AIStates.Traverse:
                    commandBufferParallel.RemoveComponent<TraverseActionTag>(chunkIndex, Self);
                    break;
                case AIStates.Wait:
                    commandBufferParallel.RemoveComponent<WaitActionTag>(chunkIndex, Self);
                    break;
                case AIStates.WanderQuadrant:
                    commandBufferParallel.RemoveComponent<WanderActionTag>(chunkIndex, Self);
                    break;
                case AIStates.Attack:
                    commandBufferParallel.RemoveComponent<AttackActionTag>(chunkIndex, Self);
                    break;

                case AIStates.RetreatToLocation:
                    commandBufferParallel.RemoveComponent<RetreatActionTag>(chunkIndex, Self);
                    break;
                case AIStates.Retreat:
                    commandBufferParallel.RemoveComponent<RetreatActionTag>(chunkIndex, Self);
                    break;
            }

            //add new action tag
            switch (highScoreState)
            {

                case AIStates.Patrol:
                    commandBufferParallel.AddComponent(chunkIndex, Self,
                        new PatrolActionTag() { UpdateWayPoint = false });
                    break;
                case AIStates.Traverse:
                    commandBufferParallel.AddComponent(chunkIndex, Self,
                        new TraverseActionTag() { UpdateWayPoint = false });
                    break;
                case AIStates.WanderQuadrant:
                    commandBufferParallel.AddComponent<WanderActionTag>(chunkIndex, Self);
                    break;
                case AIStates.Wait:
                    commandBufferParallel.AddComponent<WaitActionTag>(chunkIndex, Self);
                    break;
                case AIStates.Attack:
                    commandBufferParallel.AddComponent<AttackActionTag>(chunkIndex, Self);
                    break;
                case AIStates.Retreat:
                    commandBufferParallel.AddComponent<RetreatActionTag>(chunkIndex, Self);
                    break;
            }

            brain.ValueRW.CurrentState = highScoreState;
        }

    }

}
