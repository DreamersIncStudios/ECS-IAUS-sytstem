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
            var data = new TransformComponenet { };
            dstManager.AddComponentData(entity, data);
        }

    }

    public struct TransformComponenet : IComponentData { }

    [ExecuteAlways]
    [UpdateInGroup(typeof(Unity.Transforms.TransformSystemGroup))]
    [UpdateBefore(typeof(TRSToLocalToWorldSystem))]
    public partial class CopyTransformFromInjectedGameObjectECS2 : SystemBase
    {
        EntityQuery TransformsToUpdate;
        EntityQuery m_TransformGroup;
        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            m_TransformGroup = GetEntityQuery(
                 ComponentType.ReadOnly(typeof(TransformComponenet)),
                     typeof(Transform), ComponentType.ReadWrite<LocalToWorld>());

            RequireForUpdate(m_TransformGroup);
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            var transforms = m_TransformGroup.GetTransformAccessArray();
            var transformStashes = new NativeArray<TransformStash>(transforms.length, Allocator.TempJob);
            systemDeps = new StashTransforms()
            {
                transformStashes = transformStashes
            }.Schedule(transforms, systemDeps);
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new CopyTransforms()
            {
                transformStashes = transformStashes,
                LocalChunk = GetComponentTypeHandle<LocalToWorld>(false)
            }.ScheduleSingle(m_TransformGroup, systemDeps);
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;


        }


        struct TransformStash
        {
            public float3 position;
            public quaternion rotation;
        }
        [BurstCompile]
        struct StashTransforms : IJobParallelForTransform
        {
            public NativeArray<TransformStash> transformStashes;
            public void Execute(int index, TransformAccess transform)
            {
                transformStashes[index] = new TransformStash
                {
                    rotation = transform.rotation,
                    position = transform.position,
                };
            }
        }
        [BurstCompile]
        struct CopyTransforms : IJobChunk
        {
            [DeallocateOnJobCompletion] public NativeArray<TransformStash> transformStashes;
            public ComponentTypeHandle<LocalToWorld> LocalChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<LocalToWorld> localToWorlds = chunk.GetNativeArray(LocalChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    LocalToWorld localToWorld = localToWorlds[i];
                    var transformStash = transformStashes[i];
                    localToWorld = new LocalToWorld
                    {
                        Value = float4x4.TRS(
                            transformStash.position,
                            transformStash.rotation,
                            new float3(1.0f, 1.0f, 1.0f)
                            )
                    };
                    localToWorlds[i] = localToWorld;
                }
            }
        }
    }
}

