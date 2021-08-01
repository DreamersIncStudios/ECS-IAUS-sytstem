using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace AISenses {
    public static class SoundResponses
    {
        public static Dictionary<int2, SoundResponse> SoundResponseDictionary;

        public static void CreateSoundResponseDictionary() // This need to be read in from JSON/XML file format

        {
            SoundResponseDictionary = new Dictionary<int2, SoundResponse>();
            SoundResponseDictionary.Add(new int2((int)Jobs.Citizen, (int)SoundTypes.Whistle), new SoundResponse()
            { AlertLevel = .25f,
            CautionLevel = .25f}
            );
            SoundResponseDictionary.Add(new int2((int)Jobs.Citizen, (int)SoundTypes.Explosion), new SoundResponse()
            {
                AlertLevel = 10f,
                CautionLevel = 100f
            });
            SoundResponseDictionary.Add(new int2((int)Jobs.Cop, (int)SoundTypes.Explosion), new SoundResponse()
            {
                AlertLevel = 100f,
                CautionLevel = 10f
            });
        }

    }
    public struct SoundResponse
    {
        public float AlertLevel; // using for investigate
        public float CautionLevel; // used for Retreat/Run away from 
        public float ModAlertLevel(float SoundRatio, int SecurityLevel)
        {
            float mod = SoundRatio * SecurityLevel;
            return AlertLevel * mod;
        } // using for investigate
        public float ModCautionLevel(float SoundRatio, int SecurityLevel)
        {
            float mod = SoundRatio * SecurityLevel;
            return CautionLevel * mod;
        } // used for Retreat/Run away from 

    }

    public enum Jobs { 
        none, Citizen, Cop, aldja
    }
    public enum SoundTypes {
        none, Ambient, Alarm, Whistle, RockThrow, FootStep, Fighting, GunShot, Explosion, CarBurnout // Keep adding
    }
}