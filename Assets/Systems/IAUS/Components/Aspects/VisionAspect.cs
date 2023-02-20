using AISenses;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;


namespace IAUS.ECS.Component
{
    public readonly partial struct VisionAspect : IAspect
    {
        readonly TransformAspect Transform;
        readonly DynamicBuffer<ScanPositionBuffer> ScanPositions;


        public bool TargetInRange { get {
                if (ScanPositions.IsEmpty)
                    return false;
                else
                { 
                    
                }
                    return false;    
            } 
        }
    }
}