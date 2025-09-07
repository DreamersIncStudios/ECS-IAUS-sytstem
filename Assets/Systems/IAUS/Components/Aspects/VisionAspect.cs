using Global.Component;
using System.Linq;
using DreamersIncStudio.FactionSystem;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AISenses.VisionSystems
{
    public readonly partial struct VisionAspect : IAspect
    {
        private readonly DynamicBuffer<ScanPositionBuffer> scanPositions;
        private readonly RefRW<Vision> vision;

        public float3 TargetEnemyPosition => vision.ValueRO.TargetEnemyPosition;
        public float3 TargetFriendPosition => vision.ValueRO.TargetFriendlyPosition;
        

        public Entity TargetEntity(TargetAlignmentType type)
        {
            TargetEnemyTargetInRange();
            FriendlyInRange();
            return vision.ValueRO.TargetEntity(type);
        }

        public float3 TargetPosition(TargetAlignmentType type)
        {
            TargetEnemyTargetInRange();
            FriendlyInRange();

            return vision.ValueRO.TargetPosition(type);
        }

        public bool TargetInReactRange
        {
            get
            {
                foreach (var item in scanPositions)
                    if (item is { dist: < 25, target: { Affinity: Affinity.Hate or Affinity.Negative } } )
                    {
                        return true;
                    }

                return false;
            }

        }

        private bool TargetEnemyTargetInRange()
        {
            return TargetEnemyTargetInRange(out _, out _, out _);
        }

        public bool TargetEnemyTargetInRange(out float dist)
        {
            return TargetEnemyTargetInRange(out _, out _, out dist);
        }

        public bool TargetEnemyTargetInRange(out float3 targetPosition, out float dist)
        {
            return TargetEnemyTargetInRange(out targetPosition,out _, out dist);
        }

        public bool TargetEnemyTargetInRange(out AITarget target, out float dist)
        {
            return TargetEnemyTargetInRange(out _,out target, out dist);
        }

        private bool TargetEnemyTargetInRange(out float3 Position, out AITarget target, out float dist)
        {
            target = new AITarget();
            dist = 0f;
            Position = float3.zero;
            if (scanPositions.IsEmpty)
            {
                vision.ValueRW.TargetEnemyEntity = Entity.Null;
                return false;
            }

            foreach (var scan in scanPositions)
            {
                if (scan.target.Affinity is Affinity.Love or Affinity.Positive or Affinity.Neutral) continue;
                target = scan.target.TargetInfo;
                dist = scan.target.DistanceTo;
                vision.ValueRW.TargetEnemyEntity = scan.target.Entity;
                Position = vision.ValueRW.TargetEnemyPosition =
                    vision.ValueRW.LastKnownPositionEnemy = scan.target.LastKnownPosition;
                return true;
            }

            return false;
        }

        private bool FriendlyInRange()
        {
                if (scanPositions.IsEmpty)
                    return false;
                else
                {
                    foreach (var target in scanPositions)
                    {
                        if (target.target.Affinity is Affinity.Love or Affinity.Positive)
                            return true;
                    }
                }
                return false;
            
        }

        public Target GetClosestEnemy()
        {
            var visibleTargetInArea = scanPositions.ToNativeArray(Allocator.Temp);
            visibleTargetInArea.Sort(new SortScanPositionByDistance());
            foreach (var target in visibleTargetInArea)
            {
                if (target.target.Affinity is Affinity.Hate or Affinity.Negative)
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
            foreach (var target in visibleTargetInArea.Where(target => target.target.Affinity is Affinity.Love or Affinity.Positive))
            {
                return target.target;
            }

            return new Target();
        }
    }
}