using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;



namespace DreamersIncStudio.GAIACollective.Streaming.SceneManagement.SectionMetadata
{
    // Adds each circle to the metadata entity of its section.
    // (It is assumed each circle belongs to a different section.)
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    [UpdateAfter(typeof(SubSceneBakingSystem))]
    partial struct OperationAreaBakingSystem: ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Cleanup from previous baking.
            var cleanupQuery = SystemAPI.QueryBuilder().WithAll<GaiaOperationArea, SectionMetadataSetup>().Build();
            state.EntityManager.RemoveComponent<GaiaOperationArea>(cleanupQuery);

            var gaiaQuery = SystemAPI.QueryBuilder().WithAll<GaiaOperationArea, SceneSection>().Build();
            var gaiaOperationArea = gaiaQuery.ToComponentDataArray<GaiaOperationArea>(Allocator.Temp);
            var gaiaEntities = gaiaQuery.ToEntityArray(Allocator.Temp);

            var sectionQuery = SystemAPI.QueryBuilder().WithAll<SectionMetadataSetup>().Build();

            for (var index = 0; index < gaiaEntities.Length; ++index)
            {
                var sceneSection = state.EntityManager.GetSharedComponent<SceneSection>(gaiaEntities[index]);
                var sectionEntity = SerializeUtility.GetSceneSectionEntity(sceneSection.Section, state.EntityManager,
                    ref sectionQuery, true);
                state.EntityManager.AddComponentData(sectionEntity, gaiaOperationArea[index]);
            }
        }
    }
}