using IAUS.ECS.Component;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

public partial struct IAUSCooldown : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        new UpdatePatrol() { deltaTime= deltaTime}.ScheduleParallel();
        new UpdateTraverse() { deltaTime = deltaTime }.ScheduleParallel();
        new UpdateWait() { deltaTime = deltaTime }.ScheduleParallel();
        new UpdateWander() { deltaTime = deltaTime }.ScheduleParallel();
        new UpdateAttack() { deltaTime = deltaTime}.ScheduleParallel();
    }
    [BurstCompile]

    partial struct UpdatePatrol : IJobEntity
    {
        public float deltaTime;
        void Execute(ref Patrol state)
        {
            if (state.InCooldown) {
                state.ResetTime -= deltaTime;
            }
            if (state.Status == ActionStatus.CoolDown && state.ResetTime <= 0.0f) {
                state.Status = ActionStatus.Idle;
                state.ResetTime = 0.0f;

            }
        }

    }
    [BurstCompile]

    partial struct UpdateWait : IJobEntity
    {
        public float deltaTime;

        void Execute(ref Wait state)
        {
            if (state.InCooldown)
            {
                state.ResetTime -= deltaTime;
            }
            if (state.Status == ActionStatus.CoolDown && state.ResetTime <= 0.0f)
            {
                state.Status = ActionStatus.Idle;
                state.ResetTime = 0.0f;

            }
        }

    }
    [BurstCompile]

    partial struct UpdateTraverse : IJobEntity
    {
        public float deltaTime;

        void Execute(ref Traverse state)
        {
            if (state.InCooldown)
            {
                state.ResetTime -= deltaTime;
            }
            if (state.Status == ActionStatus.CoolDown && state.ResetTime <= 0.0f)
            {
                state.Status = ActionStatus.Idle;
                state.ResetTime= 0.0f;
            }
        }

    }
    [BurstCompile]

    partial struct UpdateWander : IJobEntity
    {
        public float deltaTime;

        void Execute(ref WanderQuadrant state)
        {
            if (state.InCooldown)
            {
                state.ResetTime -= deltaTime;
            }
            if (state.Status == ActionStatus.CoolDown && state.ResetTime <= 0.0f)
            {
                state.Status = ActionStatus.Idle;
                state.ResetTime = 0.0f;
            }
        }

    }
    
    [BurstCompile]
    partial struct UpdateAttack : IJobEntity
    {
        public float deltaTime;

        void Execute(ref AttackState state)
        {
            if (state.InCooldown)
            {
                state.ResetTime -= deltaTime;
            }
            if (state.Status == ActionStatus.CoolDown && state.ResetTime <= 0.0f)
            {
                state.Status = ActionStatus.Idle;
                state.ResetTime = 0.0f;
            }
        }

    }
}
