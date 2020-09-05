using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Stats;
using Unity.Mathematics;
using Unity.Collections;

namespace CharacterAlignmentSystem
{
    public interface Senses : IComponentData
    {
        int DetectionRate { get; } // frame count phasing 
        int AlertRate { get; set; } // once the enemy is detected how fast is the alert raise
        void InitializeSense(BaseCharacter baseCharacter);

    }


    public struct Vision : Senses
    {
        public int DetectionRate { get { return 10; } }

        public int AlertRate { get; set; }
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

        public void InitializeSense(BaseCharacter baseCharacter)
        {
            AlertRate = baseCharacter.GetAbility((int)AbilityName.Detection).AdjustBaseValue;
        }

    }

    public struct ScanPositionBuffer : IBufferElementData {
        public Target target;

        public static implicit operator Target(ScanPositionBuffer e) { return e; }
        public static implicit operator ScanPositionBuffer(Target e) { return new ScanPositionBuffer { target = e }; }
    }
    public struct Target {
        public float3 position;
    
    }


}