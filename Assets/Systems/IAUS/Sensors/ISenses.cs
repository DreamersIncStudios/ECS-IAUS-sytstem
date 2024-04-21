using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Stats;
using Unity.Mathematics;
using Global.Component;
using IAUS.Core.GOAP;
using PixelCrushers.LoveHate;
using Stats.Entities;
using Unity.Burst;

namespace AISenses
{

    [System.Serializable]
    public struct Vision : ISensor
    {
        public float DetectionRange { get; set; }
        public float Timer { get; set; } // consider using Variable Rate Manager;
        public bool IsInRange(TargetAlignmentType alignmentType) => !TargetPosition(alignmentType).Equals( float3.zero);

        public bool UpdateTargetPosition(TargetAlignmentType alignmentType) =>
            !LastKnownPosition(alignmentType).Equals(TargetPosition(alignmentType)) || !LastKnownPosition(alignmentType).Equals(float3.zero);

        public Entity TargetEntity(TargetAlignmentType alignmentType)
        {
            return alignmentType switch
            {
                TargetAlignmentType.Enemy => TargetEnemyEntity,
                TargetAlignmentType.Friendly => TargetFriendlyEntity,
                _ => Entity.Null
            };
        }

        public float3 TargetPosition(TargetAlignmentType alignmentType)
        {
            return alignmentType switch
            {
                TargetAlignmentType.Enemy => TargetEnemyPosition,
                TargetAlignmentType.Friendly => TargetFriendlyPosition,
                _ => float3.zero
            };
        }

        public float3 LastKnownPosition(TargetAlignmentType alignmentType)
        {
            return alignmentType switch
            {
                TargetAlignmentType.Enemy => LastKnownPositionEnemy,
                TargetAlignmentType.Friendly => LastKnownPositionFriendly,
                _ => float3.zero
            };
        }

        public Entity TargetEnemyEntity { get; set; }
        public Entity TargetFriendlyEntity { get; set; }
 
        [SerializeField] public float3 TargetEnemyPosition { get; set; }
        public float3 LastKnownPositionEnemy { get; set; }
        public float3 TargetFriendlyPosition { get; set; }
        public float3 LastKnownPositionFriendly { get; set; }
        public int DetectionRate
        {
            get
            {
                int returnValue = new int();
                switch (EnemyAwarenessLevel)
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
        public int EnemyAwarenessLevel;  // Character alert level
        public float3 HeadPositionOffset;
        public float3 ThreatPosition;

        public float ViewRadius;
        [Range(0, 360)]
        public int ViewAngle;
        public float EngageRadius;
        public float AlertModifer; // If AI is on high alert they will notice the enemy sooner
        public void InitializeSense(BaseCharacterComponent baseCharacter)
        {
            AlertRate = baseCharacter.GetAbility((int)AbilityName.Detection).AdjustBaseValue;
            ViewRadius = 250;
            ViewAngle = 120;
            EngageRadius = 50;
            AlertModifer = 1;
        }
        public void UpdateSense(BaseCharacterComponent baseCharacter)
        {
            AlertRate = baseCharacter.GetAbility((int)AbilityName.Detection).AdjustBaseValue;
            ViewRadius = 250;
            ViewAngle = 120;
            EngageRadius = 50;
            AlertModifer = 1;
        }

    }
    [InternalBufferCapacity(0)]
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
        return x.dist.CompareTo(y.dist);
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
        public Entity Entity;
        public bool IsFriendly;
        [BurstDiscard]
        public void CheckIsFriendly(int factionID)
        {
            IsFriendly = factionID == TargetInfo.FactionID ||
                         LoveHate.factionDatabase.GetFaction(factionID).GetPersonalAffinity(TargetInfo.FactionID) > 51;
         
        }

        public AITarget TargetInfo;
        public float DistanceTo;
        public float3 LastKnownPosition;
        public bool CanSee;
        public int LookAttempt;
        public bool CantFind => LookAttempt > 3;
        public float PerceptilabilityScore;
    }

    public enum TargetAlignmentType
    {
        All,Enemy, Friendly
    }
}