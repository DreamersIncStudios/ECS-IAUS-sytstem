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
        state.RequireForUpdate<RunningTag>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
    }

    partial struct IAUSCooldownState : IJobEntity
    {
        public float DT;
        public void Execute(DynamicBuffer<StateData> states)
        {
            for (var index = 0; index < states.Length; index++)
            {
                var state = states[index];
                if (state.Status is not (ActionStatus.Running or ActionStatus.Idle))
                {
                    state.ResetTime -= DT;
                }
                states[index] = state;
            }
        }
    }




}
