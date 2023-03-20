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


        public bool TargetInRange(out float dist)
        {

            if (ScanPositions.IsEmpty)
            {
                dist = 0f;
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