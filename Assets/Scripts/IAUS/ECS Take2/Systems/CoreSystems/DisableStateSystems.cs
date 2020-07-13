using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Components.MovementSystem;

using IAUS.Core;
namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateScore))]

    [UpdateBefore(typeof(StateScoreSystem))]
    public class DisableStateSystems : JobComponentSystem
    {
        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }
        protected override JobHandle OnUpdate( JobHandle inputDeps)
        {
            ComponentDataFromEntity<Patrol> PatrolFromEntity = GetComponentDataFromEntity<Patrol>(false);
            ComponentDataFromEntity<WaitTime> Wait = GetComponentDataFromEntity<WaitTime>(false);
            ComponentDataFromEntity<Movement> move = GetComponentDataFromEntity<Movement>(false);


            EntityCommandBuffer.Concurrent entityCommandBuffer = entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
         return   Entities
               .WithNativeDisableParallelForRestriction(PatrolFromEntity)
                .WithNativeDisableParallelForRestriction(Wait)
                .WithNativeDisableParallelForRestriction(move)
                .ForEach((Entity entity, int nativeThreadIndex, ref DynamicBuffer <StateBuffer> State, ref BaseAI AI) =>
            {
            //Update states when a state finishes based on states in Map
            if (AI.CurrentState.Status == ActionStatus.Disabled)
                {

                    Patrol Ptemp = PatrolFromEntity.Exists(entity) ? PatrolFromEntity[entity] : new Patrol();
                    WaitTime WTemp = Wait.Exists(entity) ? Wait[entity] : new WaitTime();
                    switch (AI.CurrentState.StateName)
                    {
                        case AIStates.Patrol:
                            entityCommandBuffer.RemoveComponent<PatrolActionTag>(nativeThreadIndex, entity);
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
            }).Schedule(inputDeps);
        }
    }
}
