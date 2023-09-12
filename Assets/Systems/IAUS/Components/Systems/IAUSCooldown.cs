using IAUS.ECS.Component;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

public partial struct IausCooldown : ISystem
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
        new UpdatePatrol() { DeltaTime= deltaTime}.ScheduleParallel();
        new UpdateTraverse() { DeltaTime = deltaTime }.ScheduleParallel();
        new UpdateWait() { DeltaTime = deltaTime }.ScheduleParallel();
        new UpdateWander() { DeltaTime = deltaTime }.ScheduleParallel();
        new UpdateAttack() { DeltaTime = deltaTime }.ScheduleParallel();
    }
    [BurstCompile]

    partial struct UpdatePatrol : IJobEntity
    {
        public float DeltaTime;
        void Execute(ref Patrol state)
        {
            if (state.InCooldown) {
                state.ResetTime -= DeltaTime;
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
        public float DeltaTime;

        void Execute(ref Wait state)
        {
            if (state.InCooldown)
            {
                state.ResetTime -= DeltaTime;
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
        public float DeltaTime;

        void Execute(ref Traverse state)
        {
            if (state.InCooldown)
            {
                state.ResetTime -= DeltaTime;
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
        public float DeltaTime;

        void Execute(ref WanderQuadrant state)
        {
            if (state.InCooldown)
            {
                state.ResetTime -= DeltaTime;
            }
            if (state.Status == ActionStatus.CoolDown && state.ResetTime <= 0.0f)
            {
                state.Status = ActionStatus.Idle;
                state.ResetTime = 0.0f;
            }
        }

    }


    partial struct UpdateAttack: IJobEntity
    {
        public float DeltaTime;

        void Execute(ref AttackState state)
        {
            if (state.InCooldown)
            {
                state.ResetTime -= DeltaTime;
            }
            if (state.Status == ActionStatus.CoolDown && state.ResetTime <= 0.0f)
            {
                state.Status = ActionStatus.Idle;
                state.ResetTime = 0.0f;
            }
        }

    }
}
