using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
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
            MaintNeeded = MaintPlan.None;
            CompletedPercentage = 0;
            CheckInfluencePos = float3.zero;
            InfluenceAtPoint = int2.zero;
        }


        public float3 HomeLocation;
        public float CompletedPercentage { get; set; }
        public float TotalScore { get; set; }
        public AIStates Name => AIStates.PerformMaintenance;
        public ActionStatus Status { get; set; }
        public float CoolDownTime { get; set; }
        public bool InCooldown { get; set; }
        public float ResetTime { get; set; }
        public float mod => 1.0f - 1.0f / 4.0f;
        public int Index { get; private set; }
        public bool AllTaskComplete => MaintenancePlan.Length == 0;
        public int2 InfluenceAtPoint;
        public float3 CheckInfluencePos;

        public void SetIndex(int index)
        {
            Index = index;
        }

        public Entity EntityWhichNeedsMaintenance;
        public float3 MaintLocation;
        public MaintPlan MaintNeeded;
        public FixedList32Bytes<MaintPlan> MaintenancePlan;
    }

    public struct MaintenanceTag : IComponentData
    {
        public Entity ObjectToPickup;
    }

    public enum MaintPlan
    {
        None,
        Rest,
        GetAmmo,
        Refuel,
        LocateFuel,
        MoveToObject,
        GetParts,
        GetTools,
        Repair,
        Rebuild,
        Upgrade,
        Destroy,
        ReloadMachine,
        GotoMachine
    }

    public struct AddWorkTag : IComponentData
    {
        public Entity entity;
        public MaintPlan plan;
        public float3 location;

        public AddWorkTag(Entity entity, MaintPlan plan, float3 location)
        {
            this.entity = entity;
            this.plan = plan;
            this.location = location;
        }
    }
}