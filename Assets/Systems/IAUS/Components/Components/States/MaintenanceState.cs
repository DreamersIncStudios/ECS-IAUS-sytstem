using System;
using Components.MovementSystem;
using ProjectDawn.Navigation;
using Stats.Entities;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public struct MaintenanceState : IBaseStateScorer
    {
        public MaintenanceState(float coolDownTime, float resetTime)
        {
            CoolDownTime = coolDownTime;
            Status = ActionStatus.Idle;
            Index = 0;
            ResetTime = resetTime;
            TotalScore = 0;
            MaintenancePlan = new FixedList32Bytes<MaintPlan>();
            MaintLocation = float3.zero;
            EntityWhichNeedsMaintenance = Entity.Null;
            InCooldown = false;
            HomeLocation = float3.zero;
        }
        public float3 HomeLocation;
        public float TotalScore { get; set; }
        public AIStates Name => AIStates.PerformMaintenance;
        public ActionStatus Status { get; set; }
        public float CoolDownTime { get; set; }
        public bool InCooldown { get; set; }
        public float ResetTime { get; set; }
        public float mod => 1.0f-1.0f/4.0f;
        public int Index { get; private set; }
        public bool AllTaskComplete => MaintenancePlan.Length == 0;
        public void SetIndex(int index)
        {
            Index = index;
        }

        public Entity EntityWhichNeedsMaintenance;
        public float3 MaintLocation;
        public FixedList32Bytes<MaintPlan> MaintenancePlan;
    }
    public struct MaintenanceTag : IComponentData { }
    public enum MaintPlan
    {
        None, Rest, GotoItem, Refuel, GetFuel, GetParts, GetTools, Repair, Rebuild, Upgrade, Destroy,
    }
    
    public readonly partial struct  MaintAspect
    {
        private readonly RefRW<MaintenanceState> state;
        private readonly RefRO<LocalTransform> transform;
        private readonly RefRO<AIStat> stats;
        private readonly RefRW<Movement> move;
        private readonly RefRO<AgentBody> agent;

        private bool IsHealthy => stats.ValueRO.HealthRatio > .725f;
        private bool IsInDanger => stats.ValueRO.HealthRatio < .35f;

        public void ExecutePlan(Entity entity, int chunkIndex, float deltaTime, EntityCommandBuffer.ParallelWriter ecb)
        {
            if (state.ValueRO.MaintenancePlan.IsEmpty)
                return;
            switch (state.ValueRO.MaintenancePlan[0])
            {
                case MaintPlan.None:
                    break;
                case MaintPlan.Rest:
                    break;
                case MaintPlan.Refuel:
                    Debug.Log("Adding fuel");
                    break;
                case MaintPlan.GetFuel:
                    Debug.Log("Need to get fuel");
                    break;
                case MaintPlan.GetParts:
                    Debug.Log("Need to get parts");
                    break;
                case MaintPlan.GetTools:
                    Debug.Log("Need to get tools");
                    break;
                case MaintPlan.Repair:
                    Debug.Log("Fixing things");
                    break;
                case MaintPlan.Rebuild:
                    Debug.Log("Rebuilding things");
                    break;
                case MaintPlan.Upgrade:
                    Debug.Log("Upgrading things");
                    break;
                case MaintPlan.Destroy:
                    Debug.Log("Destroying things");
                    break;
                case MaintPlan.GotoItem:
                    
                    if(!move.ValueRO.TargetLocation.Equals(state.ValueRO.MaintLocation) && !state.ValueRO.MaintLocation.Equals(float3.zero))
                        move.ValueRW.SetLocation(state.ValueRO.MaintLocation);
                    if(agent.ValueRO.RemainingDistance<5)
                        state.ValueRW.MaintenancePlan.RemoveAt(0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}