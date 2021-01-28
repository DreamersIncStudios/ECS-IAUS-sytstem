using Unity.Burst;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;
using InfluenceSystem.Component;
public class TransformConversion : MonoBehaviour,IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<CopyTransformFromGameObject>(entity);
    }

}

public class stupidSystem : SystemBase
{
    EntityQuery stupidshit;
    EntityCommandBufferSystem entityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        stupidshit = GetEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] { ComponentType.ReadOnly(typeof(LocalToWorld)), ComponentType.ReadWrite(typeof(Translation))},
            None = new ComponentType[] { ComponentType.ReadOnly(typeof(InfluenceGridPoint))} 
        });

    }
    protected override void OnUpdate()
    {
        JobHandle systemDeps = Dependency;
        systemDeps = new TranslationUpdate()
        {
            LocalChunk = GetComponentTypeHandle<LocalToWorld>(true),
            TranslationChunk = GetComponentTypeHandle<Translation>(false)
        }.ScheduleParallel(stupidshit, systemDeps);
        entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
        Dependency = systemDeps;


    }

    [BurstCompile]
    struct TranslationUpdate : IJobChunk {
        public ComponentTypeHandle<Translation> TranslationChunk;
        [ReadOnly] public ComponentTypeHandle<LocalToWorld> LocalChunk;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {

            NativeArray<Translation> translations = chunk.GetNativeArray(TranslationChunk);
            NativeArray<LocalToWorld> toWorlds = chunk.GetNativeArray(LocalChunk);
            for (int i = 0; i  < chunk.Count; i ++)
            {
                Translation tran = translations[i];
                tran.Value = toWorlds[i].Position;
                translations[i] = tran;

            }
        }
    }

}
