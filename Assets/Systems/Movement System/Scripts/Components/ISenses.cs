using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Stats;
using Unity.Mathematics;
using Unity.Collections;
using Global.Component;

namespace AISenses
{
    public interface ISenses : IComponentData
    {
        void InitializeSense(BaseCharacter baseCharacter);
        void UpdateSense(BaseCharacter baseCharacter);

    }

    [System.Serializable]
    public struct Vision : ISenses
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

        [Range(0, 5)]
        public int EnemyAwarnessLevel;  // Character alert level
        public float3 HeadPositionOffset;
        public float3 ThreatPosition;

        public float viewRadius;
        [Range(0, 360)]
        public int ViewAngle;
        public float EngageRadius;
        public float AlertModifer; // If AI is on high alert they will notice the enemy sooner
        public void InitializeSense(BaseCharacter baseCharacter)
        {
            AlertRate = baseCharacter.GetAbility((int)AbilityName.Detection).AdjustBaseValue;
        }
        public void UpdateSense(BaseCharacter baseCharacter) { }

    }
    [InternalBufferCapacity(100)]
    public struct ScanPositionBuffer : IBufferElementData
    {
        public Target target;
       public float dist;

        public static implicit operator Target(ScanPositionBuffer e) { return e; }
        public static implicit operator ScanPositionBuffer(Target e) { return new ScanPositionBuffer { target = e }; }
    }

    public struct SortScanPositionByDistance : IComparer<ScanPositionBuffer>
    {
        public int Compare(ScanPositionBuffer x, ScanPositionBuffer y)
        {
            if (x.dist > y.dist)
                return 1;
            else
                return -1;
        }
    }

    public struct HitDistanceComparer : IComparer<ScanPositionBuffer>
    {
        public int Compare(ScanPositionBuffer lhs, ScanPositionBuffer rhs)
        {
            return lhs.dist.CompareTo(rhs.dist);
        }
    }

    public struct Target
    {
        public Entity entity;
        public AITarget TargetInfo;
        public float DistanceTo;
        public float3 LastKnownPosition;
        public bool CanSee;
        public int LookAttempt;
        public bool CantFind => LookAttempt > 3;
        public float PerceptilabilityScore;
    }

}