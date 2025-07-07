using Unity.Entities;
using UnityEngine;

namespace DreamersIncStudio.GAIACollective
{

    public struct GaiaTime: IComponentData
    {
        public float StartTimeOfDay;
        public float CycleSpeed;
        public float TimeOfDay;
        private bool isTimeRunning;
        private float _lastTimeUpdated;
        public GaiaTime(float authoringStartTimeOfDay, GaiaConfiguration authoringConfiguration)
        {
           StartTimeOfDay = authoringStartTimeOfDay;
           CycleSpeed = authoringConfiguration.CycleSpeed;
           TimeOfDay = StartTimeOfDay;
           isTimeRunning = true;
           _lastTimeUpdated = -1f;
        }
        public void UpdateTime(float DT)
        {
            TimeOfDay += CycleSpeed * DT;
            if (TimeOfDay >= 24f)
                TimeOfDay = 0f;

            var currentMinute = Mathf.FloorToInt((TimeOfDay - Mathf.FloorToInt(TimeOfDay)) * 60); // ✅ Get exact minute

            // ✅ Only update UI when the minute changes
            if (currentMinute != _lastTimeUpdated)
            {
                _lastTimeUpdated = currentMinute;
             
            }
        }
    }

    public struct RunningTag : IComponentData
    {
    }
}