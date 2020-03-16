using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

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
        public float distanceToClosetEnemy;
        public float distanceToClosetTarget;
        public FactionsTypes AggressionLevels; // This might need to be moved to factionTag
        public float AlertModifer;
    }
    [System.Serializable]
    public struct FactionsTypes {
        public float Humans;
        public float Daemons;
        public float Angels;
    }
}