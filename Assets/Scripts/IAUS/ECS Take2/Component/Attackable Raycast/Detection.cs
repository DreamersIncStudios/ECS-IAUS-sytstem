
using UnityEngine;
using Unity.Entities;
using InfluenceMap.Factions;

namespace IAUS.ECS2
{
    [GenerateAuthoringComponent]
    public struct Detection : IComponentData
    {
        public float viewRadius;
        [Range(0, 360)]
        public float viewAngleXZ;
        [Range(0, 360)]
        public float viewAngleYZ;
        public float EngageRadius;
        public LayerMask TargetMask;
        public LayerMask ObstacleMask;
        public float TargetVisibility;
        public Entity TargetRef;
        public float AlertModifer;
    }

}