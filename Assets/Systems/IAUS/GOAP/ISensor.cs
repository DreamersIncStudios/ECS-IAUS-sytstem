using System;
using AISenses;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
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
        public bool IsInRange { get; }
        public bool UpdateTargetPosition { get; }

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
            var transforms = SystemAPI.GetComponentLookup<LocalTransform>();
                
            Entities.WithoutBurst().ForEach((ref Vision vision) =>
            {
                if (vision.targetEntity == Entity.Null) return;
                vision.TargetPosition = transforms[vision.targetEntity].Position;
                if (vision is not { IsInRange: true, UpdateTargetPosition: true }) return;
                vision.LastKnownPosition = vision.TargetPosition;
                if (SensorChange == null) return;
                SensorChange(this, new OnTargetChanged() { });

            }).Run();
        }

    }
}