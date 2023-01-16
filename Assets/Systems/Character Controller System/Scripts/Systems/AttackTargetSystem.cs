using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using System.Linq;

namespace AISenses.VisionSystems.Combat
{
    //[UpdateInGroup(typeof(LateSimulationSystemGroup))]
    //[UpdateAfter(typeof(TargetingSystem))]
    //public partial class AttackTargetSystem : SystemBase
    //{

    //    protected override void OnUpdate()
    //    {

    //        Entities.ForEach((ref AttackTarget attackTarget, ref DynamicBuffer<ScanPositionBuffer> buffer) =>
    //        {
    //           if(buffer.Length ==0)
    //                return;
    //            if (attackTarget.isTargeting && buffer.Length >= attackTarget.AttackTargetIndex) 
    //            {
    //                attackTarget.AttackTargetLocation = buffer[attackTarget.AttackTargetIndex].target.LastKnownPosition;
    //            }
    //            else {
    //                NativeArray<ScanPositionBuffer> scans = buffer.ToNativeArray(Allocator.Temp);
    //                if (buffer.Length > 0) { 
    //                    //Attack in direction of point target
    //                    var visibleTargetInArea =  buffer.ToNativeArray(Allocator.Temp);
    //                    visibleTargetInArea.Sort(new SortScanPositionByDistance());
    //                    for (int i = 0; i < visibleTargetInArea.Length; i++)
    //                    {

    //                    }
    //                    attackTarget.AttackTargetLocation = visibleTargetInArea[0].target.LastKnownPosition;
    //                }
    //                 else
    //                {
    //                    attackTarget.AttackTargetLocation = new float3(1,1,1);
    //                }
    //                scans.Dispose();
    //            }
    //        }).ScheduleParallel();
    //    }

    //}

}