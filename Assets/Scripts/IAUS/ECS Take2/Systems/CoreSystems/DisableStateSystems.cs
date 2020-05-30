using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Components.MovementSystem;

using IAUS.Core;
namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateState))]

    [UpdateBefore(typeof(StateScoreSystem))]
    public class DisableStateSystems : ComponentSystem
    {
        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        protected override void OnUpdate()
        {
            ComponentDataFromEntity<Patrol> PatrolFromEntity = GetComponentDataFromEntity<Patrol>(false);
            ComponentDataFromEntity<WaitTime> Wait = GetComponentDataFromEntity<WaitTime>(false);
            ComponentDataFromEntity<Party> party = GetComponentDataFromEntity<Party>(false);
            ComponentDataFromEntity<Movement> move = GetComponentDataFromEntity<Movement>(false);

            EntityCommandBuffer entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer();
            Entities.ForEach((Entity entity, DynamicBuffer<StateBuffer> State, ref BaseAI AI) =>
            {
            //Update states when a state finishes based on states in Map
            if (AI.CurrentState.Status == ActionStatus.Disabled)
                {

                    Patrol Ptemp = PatrolFromEntity.Exists(entity) ? PatrolFromEntity[entity] : new Patrol();
                    WaitTime WTemp = Wait.Exists(entity) ? Wait[entity] : new WaitTime();
                    switch (AI.CurrentState.StateName)
                    {
                        case AIStates.Patrol:
                            entityCommandBuffer.RemoveComponent<PatrolActionTag>(entity);
                            Movement temp = move[entity];
                            temp.CanMove = false;
                            move[entity] = temp;
                            break;
                    }
                   
                    

                    if (PatrolFromEntity.Exists(entity))
                        PatrolFromEntity[entity] = Ptemp;
                    if (Wait.Exists(entity))
                        Wait[entity] = WTemp;
                }
            });
        }
    }
}
