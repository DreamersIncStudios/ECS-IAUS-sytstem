
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace AISenses.VisionSystems.Combat
{
    public struct AttackTarget : IComponentData
    {
        public float3 AttackTargetLocation;
        public int AttackTargetIndex;
        public bool isTargeting;
        public float2 AttackDir;
        public float MoveRange;
        public float3 MoveTo(float3 curPos)
        {
            float dist = Vector3.Distance(curPos, AttackTargetLocation);
            if (AttackTargetLocation.Equals(new float3(1, 1, 1)))
                return new float3(1, 1, 1);

            if (dist < 10)
            {
                return Vector3.MoveTowards(curPos, AttackTargetLocation, .85f);
            }
            else
            {
                float ratio = MoveRange / dist;
                return Vector3.Lerp(curPos, AttackTargetLocation, ratio);
            }
        }
    }
}