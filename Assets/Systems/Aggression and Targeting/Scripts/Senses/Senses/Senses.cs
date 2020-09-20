using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Stats;
using Unity.Mathematics;
using Unity.Collections;

namespace AISenses
{    public interface Senses : IComponentData
    {
        int DetectionRate { get; } // frame count phasing 
        int AlertRate { get; set; } // once the enemy is detected how fast is the alert raise
        void InitializeSense(BaseCharacter baseCharacter);
        void UpdateSense(BaseCharacter baseCharacter);

    }

    [System.Serializable]
    public struct Vision : Senses
    {
        public int DetectionRate { get {
                int returnValue = new int();
                switch (EnemyAwarnessLevel) {
                    case 0:
                        returnValue = 180;
                        break;
                    case 1:
                        returnValue = 90;
                        break;
                    case 2:
                        returnValue = 45;
                        break;
                    case 3:
                        returnValue = 20;
                        break;
                    case 4:
                        returnValue = 10;
                        break;
                    case 5:
                        returnValue = 5;
                        break;
                }
                return returnValue;
            } }
        public int AlertRate { get; set; }

        [Range(0, 5)]
        public int EnemyAwarnessLevel;  // Character alert level
        public float3 HeadPositionOffset;

        public float viewRadius;
        [Range(0, 360)]
        public float ViewAngle;

        public float EngageRadius;
        public LayerMask TargetMask;
        public LayerMask ObstacleMask;
        public Entity TargetRef;
        public float AlertModifer; // If AI is on high alert they will notice the enemy sooner
        
        public float TargetVisibility; //How much of the enemy can the AI see

        public void InitializeSense(BaseCharacter baseCharacter)
        {
            AlertRate = baseCharacter.GetAbility((int)AbilityName.Detection).AdjustBaseValue;
        }
        public void UpdateSense(BaseCharacter baseCharacter) { }

    }

    public struct ScanPositionBuffer : IBufferElementData {
        public Target target;

        public static implicit operator Target(ScanPositionBuffer e) { return e; }
        public static implicit operator ScanPositionBuffer(Target e) { return new ScanPositionBuffer { target = e }; }
    }
    public struct Target {
        public RaycastCommand raycastCenter;
        public RaycastCommand raycastCenterLeft;
        public RaycastCommand raycastCenterRight;

        public int HitCount;
    }
   
    [System.Serializable]
    public struct Hearing : Senses
    {
        public int DetectionRate
        {
            get
            {
                int returnValue = new int();
                switch (EnemyAwarnessLevel)
                {
                    case 0:
                        returnValue = 180;
                        break;
                    case 1:
                        returnValue = 90;
                        break;
                    case 2:
                        returnValue = 45;
                        break;
                    case 3:
                        returnValue = 20;
                        break;
                    case 4:
                        returnValue = 10;
                        break;
                    case 5:
                        returnValue = 5;
                        break;
                }
                return returnValue;
            }
        }
        public int AlertRate { get; set; }

        public float AmbientNoiseLevel;
        public float AlertNoiseLevel;
        public float AlarmNoiseLevel;
        public bool CanIHearAnAlarm { get { return AlertNoiseLevel > AmbientNoiseLevel; } }
        [Range(0, 5)]
        public int EnemyAwarnessLevel;  // Character alert level
        public void InitializeSense(BaseCharacter baseCharacter)
        {
            AlertRate = baseCharacter.GetAbility((int)AbilityName.Detection).AdjustBaseValue;
        }
        public void UpdateSense(BaseCharacter baseCharacter) { }

    }
}