using Unity.Entities;
using UnityEngine;

namespace DreamersIncStudio.GAIACollective
{
    public struct GaiaLightSettings : IComponentData
    {
        public bool EnableFogControl;
        public TimeLightingSettings Daybreak;

        public TimeLightingSettings Midday;

        public TimeLightingSettings Sunset;

        public TimeLightingSettings Night;

        public GaiaLightSettings(GaiaConfiguration configuration)
        {
            Daybreak = configuration.Daybreak;
            Midday = configuration.Midday;
            Sunset = configuration.Sunset;
            Night = configuration.Night;
            EnableFogControl = configuration.EnableFogControl;
        }
    }
}