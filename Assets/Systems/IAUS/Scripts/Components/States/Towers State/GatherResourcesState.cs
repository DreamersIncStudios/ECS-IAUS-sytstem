using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.StateBlobSystem;
using IAUS.ECS.Consideration;
using Unity.Collections;

namespace IAUS.ECS.Component
{
    public struct GatherResourcesState : IBaseStateScorer
    {
        public BlobAssetReference<AIStateBlobAsset> stateRef;
        public int Index { get; set; }
       public ConsiderationScoringData HealthRatio => stateRef.Value.Array[Index].Health;
        public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].DistanceToTarget;

        public AIStates name { get { return AIStates.GatherResources; } }

        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }

        public ActionStatus Status { get { return _status; } set { _status = value; } }

        public float CoolDownTime { get { return _coolDownTime; } }

        public bool Complete; //Todo true when level time runs out 
        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;

        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

        public float mod { get { return 1.0f - (1.0f / 2.0f); } }
        [SerializeField] public ActionStatus _status;
        [SerializeField] public float _coolDownTime;
        [SerializeField] public float _resetTime { get; set; }
        [SerializeField] public float _totalScore { get; set; }

    }

    public struct GatherResourcesTag : IComponentData { bool tag; }
    [Unity.Burst.BurstCompile]
    public struct AddGatherResourcesState : IJobChunk
    {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
        [ReadOnly] public EntityTypeHandle EntityChunk;
        public ComponentTypeHandle<GatherResourcesState> GatherChunk;
        public BufferTypeHandle<StateBuffer> StateBufferChunk;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            NativeArray<GatherResourcesState> Gathers = chunk.GetNativeArray(GatherChunk);
            BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(StateBufferChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                Entity entity = entities[i];
                GatherResourcesState c1 = Gathers[i];
                DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];
                bool add = true;
                for (int index = 0; index < stateBuffer.Length; index++)
                {
                    if (stateBuffer[index].StateName == AIStates.GatherResources)
                    {
                        add = false;
                        continue;
                    }

                }
                c1.Status = ActionStatus.Idle;

                if (add)
                {
                    stateBuffer.Add(new StateBuffer()
                    {
                        StateName = AIStates.GatherResources,
                        Status = ActionStatus.Idle
                    });
                }


                Gathers[i] = c1;
            }
        }
    }
}