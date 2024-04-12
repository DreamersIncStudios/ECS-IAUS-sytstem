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

        Entity targetEntity(TargetAlignmentType alignmentType);
        float3 TargetPosition(TargetAlignmentType alignmentType);
        float3 LastKnownPosition(TargetAlignmentType alignmentType);
        public bool IsInRange(TargetAlignmentType alignmentType);
        public bool UpdateTargetPosition(TargetAlignmentType alignmentType);

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
             /*   if (vision.targetEntity == Entity.Null) return;
                vision.TargetPosition = transforms[vision.targetEntity].Position;
                if (vision is not { IsInRange: true, UpdateTargetPosition: true }) return;
                vision.LastKnownPosition = vision.TargetPosition;
                if (SensorChange == null) return;
                SensorChange(this, new OnTargetChanged() { });
*/
            }).Run();
        }

    }
}