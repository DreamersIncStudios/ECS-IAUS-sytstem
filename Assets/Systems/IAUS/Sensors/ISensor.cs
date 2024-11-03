using System;
using AISenses;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace IAUS.Core.GOAP
{
    public interface ISensor: IComponentData
    {
        public float DetectionRange { get; set; }
        float Timer { get; set; } // consider using Variable Rate Manager;

        Entity TargetEntity(TargetAlignmentType alignmentType);
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

        }

    }
}