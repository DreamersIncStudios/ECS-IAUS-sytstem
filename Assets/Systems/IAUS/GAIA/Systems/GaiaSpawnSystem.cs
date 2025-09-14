using System;
using AISenses;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Resources = AISenses.Resources;

namespace DreamersIncStudio.GAIACollective
{
    [UpdateInGroup(typeof(GaiaUpdateGroup))]
    public partial class GaiaSpawnSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingleton<GaiaControl>(out _))
            {
                var gaiaEntity = SystemAPI.GetSingletonEntity<GaiaTime>();
                EntityManager.AddComponentData(gaiaEntity, new GaiaControl(10));
            }
            var worldManager = SystemAPI.GetSingleton<WorldManager>();

            #region Spawning

            var levelManager = SystemAPI.GetComponentLookup<GaiaLevelManager>(true);
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(World.Unmanaged);

            Entities.WithReadOnly(levelManager).ForEach((Entity entity, ref GaiaSpawnBiome biome, in LocalToWorld transform) =>
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
                            biome.LevelRange * (int)worldManager.WorldLevel, worldManager.PlayerLevel);
                    }


                    biome.SpawnData[index] = spawn;

                }


                #region Pack Spawn

                for (var i = 0; i < biome.PacksToSpawn.Length; i++)
                {
                    var packInfo = biome.PacksToSpawn[i];
                    if (packInfo.Created) continue;
                    
                    // Create and set up the pack entity via ECB to avoid structural changes during the loop.
                    var baseDataEntity = ecb.CreateEntity();
                    ecb.AddComponent<LocalTransform>(baseDataEntity, new LocalTransform
                    {
                        Position = transform.Position,
                        Scale = 1
                    });
                    ecb.AddComponent<LocalToWorld>(baseDataEntity);
                    ecb.AddBuffer<PackList>(baseDataEntity);
                    ecb.AddBuffer<Enemies>(baseDataEntity);
                    ecb.AddBuffer<Allies>(baseDataEntity);
                    ecb.AddBuffer<Resources>(baseDataEntity);
                    ecb.AddBuffer<PlacesOfInterest>(baseDataEntity);

                    switch (packInfo.PackType)
                    {
                        case PackType.Assault:
                            ecb.AddComponent(baseDataEntity, Pack.AssaultTeam(biome.BiomeID, packInfo.Size));
                            break;
                        case PackType.Support:
                            ecb.AddComponent(baseDataEntity, Pack.Support(biome.BiomeID, packInfo.Size));
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
                    packInfo.Created = true;
                    biome.PacksToSpawn[i] = packInfo;
                }
                
            }).Run();
              
            #endregion

            #endregion

        }
    }

    public struct AgentInfo
    {
       
        public Entity AgentEntity;

        public AgentInfo( Entity entity = default)
        {
            
            AgentEntity = entity;
        }
    }

}