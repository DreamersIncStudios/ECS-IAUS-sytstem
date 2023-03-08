using IAUS.ECS.Component;
using IAUS.ECS.Systems.Reactive;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.AI;
using Utilities.ReactiveSystem;

namespace Components.MovementSystem
{
    public partial class ChangeMoveSpeedSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            Entities.WithoutBurst().WithChangeFilter<PatrolActionTag>().ForEach((NavMeshAgent agent,ref Movement mover) => {
                    agent.speed = .65f;
                }).Run();
            Entities.WithoutBurst().WithChangeFilter<TraverseActionTag>().ForEach((NavMeshAgent agent, ref Movement mover) => {
                agent.speed = .65f;
            }).Run();
        }
    }
}