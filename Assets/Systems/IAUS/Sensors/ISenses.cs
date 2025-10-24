using System.Collections.Generic;
using DreamersIncStudio.FactionSystem;
using Unity.Entities;
using UnityEngine;
using Stats;
using Unity.Mathematics;
using Global.Component;
using IAUS.Core.GOAP;
using Stats.Entities;
using Unity.Burst;
// ReSharper disable FunctionRecursiveOnAllPaths

namespace AISenses
{

    [System.Serializable]
    public struct Vision : ISensor
    {
        public float DetectionRange { get; set; }
        public float Timer { get; set; } // consider using Variable Rate Manager;
      
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
                var returnValue = EnemyAwarenessLevel switch
                {
                    0 => 180,
                    1 => 90,
                    2 => 45,
                    3 => 20,
                    4 => 10,
                    5 => 5,
                    _ => 0
                };
                return returnValue;
            }
        }
        public int AlertRate { get; set; }

        [Range(0, 5)]
        public int EnemyAwarenessLevel;  // Character alert level

        public float ViewRadius;
        [Range(0, 360)]
        public int ViewAngle;
        public void InitializeSense(BaseCharacterComponent baseCharacter)
        {
            AlertRate = baseCharacter.GetAbility((int)AbilityName.Detection).AdjustBaseValue;
            ViewRadius = 250;
            ViewAngle = 120;

        }
        public void UpdateSense(BaseCharacterComponent baseCharacter)
        {
            AlertRate = baseCharacter.GetAbility((int)AbilityName.Detection).AdjustBaseValue;
            ViewRadius = 250;
            ViewAngle = 120;

        }

    }

    public interface IInteractables
    {
        public float Dist { get; }

    }

    [InternalBufferCapacity(0)]
    public struct Enemies : IBufferElementData,IInteractables
    {
        public Target Target;
        public float Dist { get; }

        public static implicit operator Target(Enemies e) { return e; }
        public static implicit operator Enemies(Target e) { return new Enemies { Target = e }; }
    }
    [InternalBufferCapacity(0)]
    public struct Allies : IBufferElementData,IInteractables
    {
        public Target Target;
        public float Dist { get; }

        public static implicit operator Target(Allies e) { return e; }
        public static implicit operator Allies(Target e) { return new Allies() { Target = e }; }
    }
    [InternalBufferCapacity(0)]
    public struct PlacesOfInterest : IBufferElementData,IInteractables
    {
        public Target Target;
        public float Dist { get; }

        public static implicit operator Target(PlacesOfInterest e) { return e; }
        public static implicit operator PlacesOfInterest(Target e) { return new PlacesOfInterest() { Target = e }; }
    }
    [InternalBufferCapacity(0)]
    public struct Resources : IBufferElementData,IInteractables
    {
        public Target Target;
        public float Dist { get; }

        public static implicit operator Target(Resources e) { return e; }
        public static implicit operator Resources(Target e) { return new Resources() { Target = e }; }
    }

    [InternalBufferCapacity(0)]
    public struct GlobalTargets : IBufferElementData
    {
        public AITarget Target { get; set; }
        public float3 CurPosition { get; set; }
        public Entity Entity { get; set; }

        public static implicit operator AITarget(GlobalTargets e)
        {
            return e;
        }

        public static implicit operator GlobalTargets(AITarget e)
        {
            return new GlobalTargets() { Target = e };
        }
    }
    
    public struct SortScanPositionByDistance : IComparer<Enemies>
    {
        public int Compare(Enemies x, Enemies y)
        {
            return x.Dist.CompareTo(y.Dist);
        }
    }

    public struct HitDistanceComparer : IComparer<Enemies>
    {
        public int Compare(Enemies lhs, Enemies rhs)
        {
            return lhs.Dist.CompareTo(rhs.Dist);
        }
    }

    public struct Target
    {
        public Entity Entity;
        public Affinity Affinity;
        public AITarget TargetInfo;
        public float DistanceTo;
        public float3 LastKnownPosition;
        public bool CanSee;
        public int LookAttempt;
        public bool CantFind => LookAttempt > 3;
        public float PerceptilabilityScore;
    }

    public struct SortTargetsByDistanceTo : IComparer<Target>
    {
        public int Compare(Target x, Target y)
        {
            return x.DistanceTo.CompareTo(y.DistanceTo);
        }
    }
}