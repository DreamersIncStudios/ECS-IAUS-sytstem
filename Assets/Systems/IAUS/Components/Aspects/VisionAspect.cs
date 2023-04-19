using AISenses;
using Global.Component;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;


namespace AISenses.VisionSystems
{
    public readonly partial struct VisionAspect : IAspect
    {
        readonly TransformAspect Transform;
        readonly DynamicBuffer<ScanPositionBuffer> ScanPositions;
        readonly  RefRO<AITarget> self;


        public bool TargetInRange()
        {

            if (ScanPositions.IsEmpty)
            {
                return false;
            }
            else
            {
                foreach (var scan in ScanPositions)
                {
                    if (!scan.target.TargetInfo.IsFriend(self.ValueRO.FactionID))
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

            if (ScanPositions.IsEmpty)
            {
                return false;
            }
            else
            {
                foreach (var scan in ScanPositions)
                {
                    if (!scan.target.TargetInfo.IsFriend(self.ValueRO.FactionID))
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

            if (ScanPositions.IsEmpty)
            {
                return false;
            }
            else
            {
                foreach (var scan in ScanPositions)
                {
                    if (!scan.target.TargetInfo.IsFriend(self.ValueRO.FactionID))
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

            if (ScanPositions.IsEmpty)
            {
                return false;
            }
            else
            {
                foreach (var scan in ScanPositions)
                {
                    if (!scan.target.TargetInfo.IsFriend(self.ValueRO.FactionID))
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
                if (ScanPositions.IsEmpty)
                    return false;
                else
                {
                    foreach (var target in ScanPositions)
                    {
                        if (target.target.TargetInfo.IsFriend(self.ValueRO.FactionID))
                            return true;
                    }
                }
                return false;
            }
        }
    }
}