//using UnityEngine;
//using Unity.Entities;
//using System;
//using IAUS.ECS.Consideration;
//using IAUS.ECS.StateBlobSystem;
//using Unity.Collections;
//using Unity.Burst.Intrinsics;

//namespace IAUS.ECS.Component
//{
//    public struct SpawnDefendersState : IBaseStateScorer
//    {
//        public BlobAssetReference<AIStateBlobAsset> stateRef;
//        public int Index;
//        public ConsiderationScoringData HealthRatio => stateRef.Value.Array[Index].Health;
//        public ConsiderationScoringData TargetInRange => stateRef.Value.Array[Index].DistanceToTarget;
//        public ConsiderationScoringData EnergyMana => stateRef.Value.Array[Index].ManaAmmo;
//        public ConsiderationScoringData NumberOfDefenders => stateRef.Value.Array[Index].ManaAmmo2;


//        public float SpawnTimer;
//        public int DefendersActive;
//        public int MaxNumberOfDefender;
//        public float DefenederRatio => 1.0f - DefendersActive / (float)MaxNumberOfDefender;
//        public AIStates name { get { return AIStates.CallBackUp; } }

//        public float TotalScore { get { return _totalScore; } set { _totalScore = value; } }

//        public ActionStatus Status { get { return _status; } set { _status = value; } }
//        public bool Complete;
//        public float CoolDownTime { get { return _coolDownTime; } }

//        public bool InCooldown => Status != ActionStatus.Running || Status != ActionStatus.Idle;

//        public float ResetTime { get { return _resetTime; } set { _resetTime = value; } }

//        public float mod { get { return 1.0f - (1.0f / 4.0f); } }
//        [SerializeField] public ActionStatus _status;
//        [SerializeField] public float _coolDownTime;
//        [SerializeField] public float _resetTime { get; set; }
//        [SerializeField] public float _totalScore { get; set; }
//    }

//    public struct SpawnTag : IComponentData { public int SpawnID; }

//    //public struct AddSpawnDefendersState : IJobChunk
//    //{
//    //    public EntityCommandBuffer.ParallelWriter entityCommandBuffer;
//    //    public ComponentTypeHandle<SpawnDefendersState> SpawnChunk;
//    //    public BufferTypeHandle<StateBuffer> StateBufferChunk;
//    //    public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
//    //    {
//    //        NativeArray<SpawnDefendersState> Spawns = chunk.GetNativeArray(ref SpawnChunk);
//    //        BufferAccessor<StateBuffer> StateBufferAccesor = chunk.GetBufferAccessor(ref StateBufferChunk);
//    //        for (int i = 0; i < chunk.Count; i++)
//    //        {
//    //            SpawnDefendersState c1 = Spawns[i];
//    //            DynamicBuffer<StateBuffer> stateBuffer = StateBufferAccesor[i];
//    //            bool add = true;
//    //            for (int index = 0; index < stateBuffer.Length; index++)
//    //            {
//    //                if (stateBuffer[index].StateName == c1.name)
//    //                {
//    //                    add = false;
//    //                    continue;
//    //                }

//    //            }
//    //            c1.Status = ActionStatus.Idle;

//    //            if (add)
//    //            {
//    //                stateBuffer.Add(new StateBuffer(c1.name));
//    //            }


//    //            Spawns[i] = c1;
//    //        }
//    //    }

   
// //   }
//}
