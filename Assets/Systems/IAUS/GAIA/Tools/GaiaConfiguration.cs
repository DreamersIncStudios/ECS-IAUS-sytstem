using UnityEngine;

namespace DreamersIncStudio.GAIACollective
{
    [CreateAssetMenu(fileName = "Config", menuName = "GAIA/Configuration", order = 1)]
    public class GaiaConfiguration : ScriptableObject
    {
        public float CycleSpeed => cycleSpeed;
        [SerializeField] private float cycleSpeed =.2f;
        public bool EnableFogControl => enableFogControl;
        [SerializeField] bool enableFogControl = true;
        public TimeLightingSettings Daybreak;

        public TimeLightingSettings Midday;

        public TimeLightingSettings Sunset;

        public TimeLightingSettings Night;

        public void SetToDaybreak(GameObject sun) => SetTimeInstantly(6f,sun);
        public void SetToMidday(GameObject sun) => SetTimeInstantly(12f,sun);
        public void SetToSunset(GameObject sun) => SetTimeInstantly(18f,sun);
        public void SetToNight(GameObject sun) => SetTimeInstantly(0f,sun);

        private void SetTimeInstantly(float targetTime, GameObject sun)
        {
            UpdateLighting( targetTime % 24f, sun);  // Apply the correct lighting immediately

        }


        // ReSharper disable Unity.PerformanceAnalysis
        private void UpdateLighting(float timeOfDay, GameObject sun)
        {
            float timePercent = timeOfDay / 24f;
            float xRotation = (timePercent * 360f) - 90f;
            var rotationPivot = sun.transform;
            if (rotationPivot.localRotation.eulerAngles.x != xRotation || rotationPivot.localRotation.eulerAngles.y != 0)
            {
                rotationPivot.localRotation = Quaternion.Euler(new Vector3(xRotation, 0, 0));
            } 
            TimeLightingSettings from, to;
            var light = sun.GetComponent<Light>();
            float blend;
            switch (timeOfDay)
            {
                case < 6f:
                    from = Night; to = Daybreak; blend = timeOfDay / 6f;
                    break;
                case < 12f:
                    from = Daybreak; to = Midday; blend = (timeOfDay- 6f) / 6f;
                    break;
                case < 18f:
                    from = Midday; to = Sunset; blend = (timeOfDay - 12f) / 6f;
                    break;
                default:
                    from = Sunset; to = Night; blend = (timeOfDay - 18f) / 6f;
                    break;
            }
            RenderSettings.ambientLight = Color.Lerp(from.ambientColor, to.ambientColor, blend);
            light.color = Color.Lerp(from.sunColor, to.sunColor, blend);
            light.intensity = Mathf.Lerp(from.sunIntensity, to.sunIntensity, blend);
            light.shadowStrength = Mathf.Lerp(from.shadowStrength, to.shadowStrength, blend);

            if (EnableFogControl && RenderSettings.fog)
            {
                RenderSettings.fogColor = Color.Lerp(from.fogColor, to.fogColor, blend);
                RenderSettings.fogDensity = Mathf.Lerp(from.fogDensity, to.fogDensity, blend);
            }
        }
    }
    [System.Serializable]
    public struct TimeLightingSettings
    {
        [InspectorName("Scene Ambient")] public Color ambientColor;
        [InspectorName("Sun Color")] public Color sunColor;
        [InspectorName("Camera Background")] public Color backgroundColor;
        [InspectorName("Sun Intensity")] public float sunIntensity;
        [InspectorName("Shadow Strength"), Range(0f, 1f)] public float shadowStrength;

        [Header("Fog Settings")]
        [InspectorName("Fog Color")] public Color fogColor;
        [InspectorName("Fog Density")] public float fogDensity;

        [Header("Water Settings")]
        [InspectorName("Water Color")] public Color waterColor;
    }
}