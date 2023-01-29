using IAUS.ECS.Component;
using IAUS.ECS.StateBlobSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;


namespace IAUS.ECS.Systems
{


    [UpdateAfter(typeof(SetupAIStateBlob))]
    public class IAUSUpdateGroup : ComponentSystemGroup
    {
        public IAUSUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(2500, true);

        }

    }

    [UpdateInGroup(typeof(IAUSUpdateGroup))]
    public  partial struct IAUSBrainUpdate : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            throw new System.NotImplementedException();
        }

        public void OnDestroy(ref SystemState state)
        {
            throw new System.NotImplementedException();
        }
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (patrol, buffer) in SystemAPI.Query<PatrolAspect, DynamicBuffer<StateBuffer>>()) {
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i].StateName == AIStates.Patrol)
                    {
                        StateBuffer temp = buffer[i];
                        temp.StateName = AIStates.Patrol;
                        temp.TotalScore = patrol.Score;
                        temp.Status = patrol.Status;
                    }
                }
            }
            foreach (var (traverse, buffer) in SystemAPI.Query<TraverseAspect, DynamicBuffer<StateBuffer>>())
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i].StateName == AIStates.Traverse)
                    {
                        StateBuffer temp = buffer[i];
                        temp.StateName = AIStates.Traverse;
                        temp.TotalScore = traverse.Score;
                        temp.Status = traverse.Status;
                    }
                }
            }
        }
    }
}