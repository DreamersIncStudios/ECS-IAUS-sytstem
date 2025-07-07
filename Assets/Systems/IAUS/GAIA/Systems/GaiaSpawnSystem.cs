using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DreamersIncStudio.GAIACollective
{
    public partial class GaiaUpdateGroup : ComponentSystemGroup
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<RunningTag>();
            RequireForUpdate<GaiaTime>();
            RequireForUpdate<WorldManager>();
        }

        public GaiaUpdateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(80, true);
        }
    }
    [UpdateInGroup(typeof(GaiaUpdateGroup))]
    public partial class GaiaSpawnSystem : SystemBase
    {
    
        protected override void OnCreate()
        {
            base.OnCreate();
        }


        protected override void OnUpdate()
        {
            if (!SystemAPI.TryGetSingleton<GaiaControl>(out _))
            {
                var gaiaEntity = SystemAPI.GetSingletonEntity<GaiaTime>();
                EntityManager.AddComponentData(gaiaEntity, new GaiaControl(10));
            }
            var worldManager = SystemAPI.GetSingleton<WorldManager>();
            var gaiaTime = SystemAPI.GetSingletonRW<GaiaTime>();
            var gaiaSettings = SystemAPI.GetSingleton<GaiaLightSettings>();
            gaiaTime.ValueRW.UpdateTime(SystemAPI.Time.DeltaTime);
            var timeOfDay = gaiaTime.ValueRO.TimeOfDay;

            #region Lighting

            Entities.WithoutBurst().ForEach((Light light) =>
            {
                var rotationPivot = light.transform;
                float timePercent = gaiaTime.ValueRO.TimeOfDay / 24f;
                float xRotation = (timePercent * 360f) - 90f;
                if (rotationPivot.localRotation.eulerAngles.x != xRotation ||
                    rotationPivot.localRotation.eulerAngles.y != -30)
                {
                    rotationPivot.localRotation = Quaternion.Euler(new Vector3(xRotation, -30, 0));
                }

                TimeLightingSettings from, to;
                float blend;
                switch (timeOfDay)
                {
                    case < 6f:
                        from = gaiaSettings.Night;
                        to = gaiaSettings.Daybreak;
                        blend = timeOfDay / 6f;
                        break;
                    case < 12f:
                        from = gaiaSettings.Daybreak;
                        to = gaiaSettings.Midday;
                        blend = (timeOfDay - 6f) / 6f;
                        break;
                    case < 18f:
                        from = gaiaSettings.Midday;
                        to = gaiaSettings.Sunset;
                        blend = (timeOfDay - 12f) / 6f;
                        break;
                    default:
                        from = gaiaSettings.Sunset;
                        to = gaiaSettings.Night;
                        blend = (timeOfDay - 18f) / 6f;
                        break;
                }

                RenderSettings.ambientLight = Color.Lerp(from.ambientColor, to.ambientColor, blend);
                light.color = Color.Lerp(from.sunColor, to.sunColor, blend);
                light.intensity = Mathf.Lerp(from.sunIntensity, to.sunIntensity, blend);
                light.shadowStrength = Mathf.Lerp(from.shadowStrength, to.shadowStrength, blend);

                if (gaiaSettings.EnableFogControl && RenderSettings.fog)
                {
                    RenderSettings.fogColor = Color.Lerp(from.fogColor, to.fogColor, blend);
                    RenderSettings.fogDensity = Mathf.Lerp(from.fogDensity, to.fogDensity, blend);
                }
            }).Run();

            #endregion

            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var endBuffer = ecb.CreateCommandBuffer(World.Unmanaged);

            #region Spawning

            var updateHashMap = false;
            Entities.WithStructuralChanges().ForEach((ref GaiaSpawnBiome biome) =>
            {
                for (var index = 0; index < biome.SpawnData.Length; index++)
                {
                    var spawn = biome.SpawnData[index];
                    switch (spawn)
                    {
                        case { IsSatisfied: true, Respawn: false }:
                            break;
                        case { Respawn: true, IsSatisfied: true }:
                          spawn.ResetRespawn();
                            break;
                        default:
                       spawn.Spawn(biome.BiomeID,biome.LevelRange*(int)worldManager.WorldLevel, worldManager.PlayerLevel);
                       updateHashMap = true;
                            break;
                    }

                    spawn.Countdown(SystemAPI.Time.DeltaTime);
                    biome.SpawnData[index] = spawn;
                }

                #region Pack Spawn
                for (var i = 0; i < biome.PacksToSpawn.Length; i++)
                {
                    var packInfo = biome.PacksToSpawn[i];
                    if(packInfo.Satisfied) continue;
                    // ReSharper disable once Unity.BurstFunctionSignatureContainsManagedTypes
                    var baseEntityArch = EntityManager.CreateArchetype(
                        new ComponentType[]
                        {
                            typeof(LocalTransform),
                            typeof(LocalToWorld)
                        }
                    );
                    var baseDataEntity = EntityManager.CreateEntity(baseEntityArch);

                    switch (packInfo.PackType)
                    {
                        case PackType.Assault:
                            EntityManager.AddComponentData(baseDataEntity, Pack.AssaultTeam(biome.BiomeID));
                           
                            break;
                        case PackType.Support:
                            EntityManager.AddComponentData(baseDataEntity, Pack.Support(biome.BiomeID));

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
                #endregion
                
            }).Run();
            if (!updateHashMap) return;
            var control = SystemAPI.GetSingleton<GaiaControl>();
            Entities.ForEach((Entity entity, ref GaiaLife life) =>
            {
                if (control.entityMapTesting.IsCreated)
                {
                    control.entityMapTesting.Clear();
                }
                control.entityMapTesting.Add(life.HomeBiomeID, new AgentInfo(entity));
            }).Schedule();


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