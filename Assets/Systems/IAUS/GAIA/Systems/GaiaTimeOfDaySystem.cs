using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;


namespace DreamersIncStudio.GAIACollective
{
    [UpdateInGroup(typeof(GaiaUpdateGroup))]
    [UpdateBefore(typeof(GaiaSpawnSystem))]
    public partial class GaiaTimeOfDaySystem : SystemBase
    {
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

            Entities.WithoutBurst().ForEach((Light light, ref LocalTransform transform) =>
            {
                if(light.type != LightType.Directional) return;
                float timePercent = gaiaTime.ValueRO.TimeOfDay / 24f;
                float xRotation = (timePercent * 360f) - 90f;
                
                // Convert transform.Rotation (quaternion) into euler angles
                float3 currentEuler = math.degrees(math.Euler(transform.Rotation));

                if (!Mathf.Approximately(currentEuler.x, xRotation) ||!Mathf.Approximately(currentEuler.y, -30))
                {
                    transform.Rotation = Quaternion.Euler(new Vector3(xRotation, -30, 0));
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
        }
        #endregion
    }
}