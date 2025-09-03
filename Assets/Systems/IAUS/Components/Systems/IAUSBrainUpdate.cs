using DreamersIncStudio.GAIACollective;
using IAUS.ECS.Component.Aspects;
using IAUS.ECS.StateBlobSystem;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace IAUS.ECS.Systems
{
    [UpdateAfter(typeof(SetupAIStateBlob))]
    [UpdateAfter(typeof(GaiaUpdateGroup))]
    public partial class IAUSUpdateGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<RunningTag>();
        }

        public IAUSUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(1000, true);

        }
    }
    public partial class IAUSUpdateStateGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<RunningTag>();
        }

    }

    [UpdateInGroup(typeof(IAUSUpdateGroup))]
    public partial struct IAUSBrainUpdate : ISystem
    {
        
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