using IAUS.ECS.Component;
using IAUS.ECS.Component.Aspects;
using IAUS.ECS.StateBlobSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace IAUS.ECS.Systems
{


    [UpdateAfter(typeof(SetupAIStateBlob))]
    public partial class IAUSUpdateGroup : ComponentSystemGroup
    {
        public IAUSUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(250, true);

        }

    }
    public partial class IAUSUpdateStateGroup : ComponentSystemGroup
    {
 
    }


    [UpdateInGroup(typeof(IAUSUpdateGroup))]
    public partial struct IAUSBrainUpdate : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }
        [BurstCompile]

        public void OnUpdate(ref SystemState state)
        {
            
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            new IAUSUpdateJob()
                    { CommandBufferParallel = ecb.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() }
                .ScheduleParallel();

        }
    }
    public partial struct IAUSUpdateJob: IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter CommandBufferParallel;
       
        void Execute([ChunkIndexInQuery] int chunkIndex,  IAUSBlackboard blackboard)
        {
            blackboard.UpdateCurrentState(CommandBufferParallel, chunkIndex);
        }
    }
    
}