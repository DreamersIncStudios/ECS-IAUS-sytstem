using IAUS.ECS.Component;
using IAUS.ECS.Systems.Reactive;
using System.Collections;
using System.Collections.Generic;
using ProjectDawn.Navigation;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;
using Utilities.ReactiveSystem;

namespace Components.MovementSystem
{
    public partial class ChangeMoveSpeedSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<RunningTag>();

        }

        protected override void OnUpdate()
        {
            Entities.WithoutBurst().WithChangeFilter<PatrolActionTag>().ForEach((ref AgentLocomotion agent, in Movement mover) =>
            {
                agent.Speed = .65f * mover.MaxMovementSpeed;
            }).Run();

            Entities.WithoutBurst().WithChangeFilter<TraverseActionTag>().ForEach((ref AgentLocomotion agent, in Movement mover) =>
            {
                agent.Speed = .65f * mover.MaxMovementSpeed;
            }).Run();
        }
    }
}