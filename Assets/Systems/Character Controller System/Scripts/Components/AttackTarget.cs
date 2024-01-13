using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace AISenses.VisionSystems.Combat
{
    /// <summary>
    /// This Component is for moving the Character toward the target it is attacking
    /// Animation Helper 
    /// </summary>
    public struct AttackTarget : IComponentData
    {
        public float3 AttackTargetLocation;
        public Entity TargetEntity { get; set; }
        public int AttackTargetIndex;
        public bool IsTargeting;
        public float2 AttackDir;
        public float MoveRange;
        public bool TargetInRange { get; set; }
        public float3 MoveTo(float3 curPos)
        {
            var dist = Vector3.Distance(curPos, AttackTargetLocation);
            if (AttackTargetLocation.Equals(new float3(1, 1, 1)))
                return new float3(1, 1, 1);

            if (dist < 10)
            {
                return Vector3.MoveTowards(curPos, AttackTargetLocation, .85f);
            }
            else
            {
                var ratio = MoveRange / dist;
                return Vector3.Lerp(curPos, AttackTargetLocation, ratio);
            }
        }
    }
}