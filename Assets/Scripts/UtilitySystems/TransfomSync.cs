using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using UnityEngine.Jobs;
using Unity.Collections;
using Unity.Transforms;

namespace ECS.Utilities
{
    public class TransfomSync : MonoBehaviour, IConvertGameObjectToEntity
    {

        public void Awake()
        {

        }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new TransformComponenet{ };
            dstManager.AddComponentData(entity, data);
        }

    }

    public struct TransformComponenet :IComponentData{ }

    [ExecuteAlways]
    [UpdateInGroup(typeof(Unity.Transforms.TransformSystemGroup))]
    [UpdateBefore(typeof(Unity.Transforms.EndFrameTRSToLocalToWorldSystem))]
    public class CopyTransformFromInjectedGameObjectECS : JobComponentSystem
    {
        struct TransformStash {
            public float3 position;
            public quaternion rotation;
        }
        [BurstCompile]
        struct StashTransforms : IJobParallelForTransform
        {
            public NativeArray<TransformStash> transformStashes;
            public void Execute(int index, TransformAccess transform) {
                transformStashes[index] = new TransformStash {
                    rotation = transform.rotation,
                    position = transform.position,
                };
            }
        }
        [BurstCompile]
        struct CopyTransforms : IJobForEachWithEntity<LocalToWorld>
        {
            [DeallocateOnJobCompletion] public NativeArray<TransformStash> transformStashes;

            public void Execute(Entity entity, int index, ref LocalToWorld localToWorld)
            {
                var transformStash = transformStashes[index];
                localToWorld = new LocalToWorld {
                    Value = float4x4.TRS(
                        transformStash.position,
                        transformStash.rotation,
                        new float3(1.0f,1.0f,1.0f)
                        )
                };


            }

        }
        EntityQuery m_TransformGroup;

        protected override void OnCreate()
        {
            m_TransformGroup = GetEntityQuery(
                ComponentType.ReadOnly(typeof(TransformComponenet)),
                typeof (Transform),
                ComponentType.ReadWrite<LocalToWorld>()
                
                );
            RequireForUpdate(m_TransformGroup);
        }



        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var transforms = m_TransformGroup.GetTransformAccessArray();
            var transformStashes = new NativeArray<TransformStash>(transforms.length, Allocator.TempJob);
            var stashTransformsJob = new StashTransforms
            {
                transformStashes = transformStashes
            };

            var stashTransformsJobHandle = stashTransformsJob.Schedule(transforms, inputDeps);

            var copyTransformsJob = new CopyTransforms
            {
                transformStashes = transformStashes,
            };


            return copyTransformsJob.Schedule(m_TransformGroup, stashTransformsJobHandle);
        }
    }
}
