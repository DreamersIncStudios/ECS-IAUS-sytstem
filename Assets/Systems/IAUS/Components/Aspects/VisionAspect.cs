using Global.Component;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace AISenses.VisionSystems
{
    public readonly partial struct VisionAspect : IAspect
    {
        private readonly RefRO<LocalTransform> transform;
        private readonly DynamicBuffer<ScanPositionBuffer> scanPositions;
        private readonly  RefRO<AITarget> self;

        public bool TargetInReactRange
        {
            get
            {
                foreach (var item in scanPositions)
                    if (item is { dist: < 25, target: { IsFriendly: false } })
                        return true;

                return false;
            }

        }

        public bool TargetInRange()
        {

            if (scanPositions.IsEmpty)
            {
                return false;
            }
            else
            {
                foreach (var scan in scanPositions)
                {
                    if (!scan.target.IsFriendly)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TargetInRange(out float dist)
        {
            dist = 0f;

            if (scanPositions.IsEmpty)
            {
                return false;
            }
            else
            {
                foreach (var scan in scanPositions)
                {
                    if (!scan.target.IsFriendly)
                    {
                        dist = scan.target.DistanceTo;
                        return true;
                    }
                }
            }
            dist = 0f;
            return false;
        }
        public bool TargetInRange(out AITarget target)
        {
            target = new AITarget();

            if (scanPositions.IsEmpty)
            {
                return false;
            }
            else
            {
                foreach (var scan in scanPositions)
                {
                    if (!scan.target.IsFriendly)
                    {
                        target = scan.target.TargetInfo;
                        return true;
                    }
                }
            }
            return false;
        }

        public bool TargetInRange(out AITarget target, out float dist)
        {
            target = new AITarget();
            dist = 0f;

            if (scanPositions.IsEmpty)
            {
                return false;
            }
            else
            {
                foreach (var scan in scanPositions)
                {
                    if (!scan.target.IsFriendly)
                    {
                        target = scan.target.TargetInfo;
                        dist = scan.target.DistanceTo;
                        return true;
                    }
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