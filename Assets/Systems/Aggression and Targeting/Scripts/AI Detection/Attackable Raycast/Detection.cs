
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
        public Entity TargetRef;
        public float AlertModifer; // If AI is on high alert they will notice the enemy sooner
        public float EnemyAwarnessLevel;  // how aware of the enemy is the AI. IE there are enemies in the area the AI has already noticed but can not longer see

        public float TargetVisibility; //How much of the enemy can the AI see

    }

}