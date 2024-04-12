using Global.Component;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace AISenses.VisionSystems
{
    public readonly partial struct VisionAspect : IAspect
    {
        private readonly DynamicBuffer<ScanPositionBuffer> scanPositions;
        private readonly RefRW<Vision> vision;

        public bool TargetInReactRange
        {
            get
            {
                foreach (var item in scanPositions)
                    if (item is { dist: < 25, target: { IsFriendly: false } })
                    {
                        return true;
                    }

                return false;
            }

        }

        public bool TargetEnemyTargetInRange()
        {
            return TargetEnemyTargetInRange(out _, out _);
        }

        public bool TargetEnemyTargetInRange(out float dist)
        {
            return TargetEnemyTargetInRange(out _, out dist);
        }
        public bool TargetEnemyTargetInRange(out AITarget target)
        {
            return TargetEnemyTargetInRange(out target, out _);
        }

        public bool TargetEnemyTargetInRange(out AITarget target, out float dist)
        {
            target = new AITarget();
            dist = 0f;

            if (scanPositions.IsEmpty)
            {
                vision.ValueRW.targetEnemyEntity = Entity.Null;
                vision.ValueRW.LastKnownPositionEnemy = float3.zero;
                return false;
            }
            else
            {
                foreach (var scan in scanPositions)
                {
                    if (scan.target.IsFriendly) continue;
                    target = scan.target.TargetInfo;
                    dist = scan.target.DistanceTo;
                    vision.ValueRW.targetEnemyEntity = scan.target.Entity;
                    vision.ValueRW.LastKnownPositionEnemy = scan.target.LastKnownPosition;
                    return true;
                }
            }
            return false;
        }

        public bool FriendlyInRange
        {
            get
            {
                if (scanPositions.IsEmpty)
                    return false;
                else
                {
                    foreach (var target in scanPositions)
                    {
                        if (target.target.IsFriendly)
                            return true;
                    }
                }
                return false;
            }
        }

        public Target GetClosestEnemy()
        {
            var visibleTargetInArea = scanPositions.ToNativeArray(Allocator.Temp);
            visibleTargetInArea.Sort(new SortScanPositionByDistance());
            foreach (var target in visibleTargetInArea)
            {
                if (!target.target.IsFriendly)
                {
                    return target.target;
                }
            }
            return new Target();
        }

        public Target GetClosestFriend()
        {
            
            var visibleTargetInArea = scanPositions.ToNativeArray(Allocator.Temp);
            visibleTargetInArea.Sort(new SortScanPositionByDistance());
            foreach (var target in visibleTargetInArea.Where(target => target.target.IsFriendly))
            {
                return target.target;
            }

            return new Target();
        }
    }
}