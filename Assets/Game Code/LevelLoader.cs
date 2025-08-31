using System;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DreamersIncStudio.GAIACollective
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial class LevelLoaderSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<WorldManager>();
            RequireForUpdate<GaiaTime>();
        }

        protected override void OnUpdate()
        {
            if (SystemAPI.HasSingleton<RunningTag>() ) return;

        var singletonEntity = SystemAPI.GetSingletonEntity<WorldManager>();

            if (!SystemAPI.TryGetSingleton<GaiaControl>(out _))
            {
                var gaiaEntity = SystemAPI.GetSingletonEntity<GaiaTime>();
                EntityManager.AddComponentData(gaiaEntity, new GaiaControl(10));
            }

            var worldManager = SystemAPI.GetSingleton<WorldManager>();

            #region Spawning
            var levelManager = SystemAPI.GetComponentLookup<GaiaLevelManager>(true);

            var updateHashMap = false;
            Entities.WithStructuralChanges().ForEach((ref GaiaSpawnBiome biome, in LocalToWorld transform) =>
            {
                if(biome.Manager == Entity.Null) return;
                var scenario = levelManager[biome.Manager].SpawnScenario;
                if(scenario == SpawnScenario.DoNotSpawn) return;

                for (var index = 0; index < biome.SpawnData.Length; index++)
                {
                    var spawn = biome.SpawnData[index];
                    if(spawn.SpawnScenario != scenario) continue;

                    spawn.Countdown(SystemAPI.Time.DeltaTime);
                    if (spawn.IsSatisfied)
                    {
                        if (spawn.Respawn)
                            spawn.ResetRespawn();
                    }
                    else if (spawn.Respawn)
                    {
                        spawn.Spawn(ref biome.SpawnRequests, biome.BiomeID,
                            biome.LevelRange * (int)worldManager.WorldLevel,
                            worldManager.PlayerLevel);
                        Debug.Log("Spawning ");
                        updateHashMap = true;
                    }


                    biome.SpawnData[index] = spawn;
                }

                #endregion

                #region Pack Spawn

                for (var i = 0; i < biome.PacksToSpawn.Length; i++)
                {
                    var packInfo = biome.PacksToSpawn[i];
                    if (packInfo.Satisfied) continue;
                    // ReSharper disable once Unity.BurstFunctionSignatureContainsManagedTypes
                    var baseEntityArch = EntityManager.CreateArchetype(
                        new ComponentType[]
                        {
                            typeof(LocalTransform),
                            typeof(LocalToWorld)
                        }
                    );
                    var baseDataEntity = EntityManager.CreateEntity(baseEntityArch);
                    EntityManager.SetName(baseDataEntity, packInfo.PackType.ToString());
                    EntityManager.SetComponentData(baseDataEntity, new LocalTransform()
                    {
                        Position = transform.Position,
                        Scale = 1
                    });
                    switch (packInfo.PackType)
                    {
                        case PackType.Assault:
                            EntityManager.AddComponentData(baseDataEntity, Pack.AssaultTeam(biome.BiomeID, packInfo.Size));

                            break;
                        case PackType.Support:
                            EntityManager.AddComponentData(baseDataEntity, Pack.Support(biome.BiomeID, packInfo.Size));

                            break;
                        case PackType.Transport:
                            break;
                        case PackType.Scavengers:
                            break;
                        case PackType.Recon:
                            break;
                        case PackType.Combat:
                            break;
                        case PackType.Acquisition:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    packInfo.Qty++;
                    biome.PacksToSpawn[i] = packInfo;
                }
            }).Run();

            #endregion

            EntityManager.AddComponent<RunningTag>(singletonEntity);
        }
    }
}