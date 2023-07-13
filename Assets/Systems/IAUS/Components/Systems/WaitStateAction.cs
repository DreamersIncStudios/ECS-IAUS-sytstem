using IAUS.ECS.Component;
using IAUS.ECS.Systems.Reactive;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Utilities.ReactiveSystem;


namespace IAUS.ECS.Systems.Reactive
{
    public partial struct WaitStateAction : ISystem
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

            new UpdateWait() { deltaTime = deltaTime }.Schedule();
        }

        [BurstCompile]

        partial struct UpdateWait : IJobEntity
        {
            public float deltaTime;

            void Execute(ref Wait wait, ref WaitActionTag tag)
            {
                wait.Timer -= deltaTime;

            }

        }
    }
}