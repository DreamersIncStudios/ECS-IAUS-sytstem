using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace IAUS.Core.GOAP
{
    public interface ISensor: IComponentData
    {
        public float detectionRange { get; set; }
        float timer { get; set; } // consider using Variable Rate Manager;

        Entity targetEntity { get; set; }
        float3 TargetPosition { get; set; }
        float3 LastKnownPosition { get; set; }
        public bool IsInRange => !TargetPosition.Equals( float3.zero);

    }

    public partial class SensorEventManagement: SystemBase
    {
        public static event EventHandler<OnTargetChanged> SensorChange;

        public class OnTargetChanged : EventArgs
        {
            public ISensor SensorComponent;
        }

        protected override void OnUpdate()
        {
            throw new NotImplementedException();
        }

    }
}