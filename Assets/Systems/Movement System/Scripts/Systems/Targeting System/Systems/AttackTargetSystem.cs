using Unity.Entities;
using Unity.Collections;

namespace AISenses.VisionSystems.Combat
{
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateAfter(typeof(TargetingSystem))]
    public partial class AttackTargetSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            Entities.ForEach((ref AttackTarget attackTarget, ref DynamicBuffer<ScanPositionBuffer> buffer, ref Vision vision) =>
            {
                if (buffer.Length == 0)
                    return;
                if (attackTarget.IsTargeting && buffer.Length >= attackTarget.AttackTargetIndex)
                {
                    attackTarget.AttackTargetLocation = buffer[attackTarget.AttackTargetIndex].target.LastKnownPosition;
                }
                else
                {
                    var scans = buffer.ToNativeArray(Allocator.Temp);
                    if (buffer.Length > 0)
                    {
                        //Attack in direction of point target
                        var visibleTargetInArea = buffer.ToNativeArray(Allocator.Temp);
                        visibleTargetInArea.Sort(new SortScanPositionByDistance());
                        var indexOf = 0;
                        for (var i = 0; i < buffer.Length; i++)
                        {
                            if (visibleTargetInArea[i].target.IsFriendly) continue;
                            indexOf = i;
                            break;
                        }
                 
                        if (vision.EngageRadius > visibleTargetInArea[indexOf].dist)
                        {
                            attackTarget.TargetEntity = visibleTargetInArea[indexOf].target.Entity; 
                            attackTarget.AttackTargetLocation = visibleTargetInArea[indexOf].target.LastKnownPosition;
                            attackTarget.TargetInRange = true;
                        }
                    }
                    else
                    {
                        attackTarget.TargetInRange = false;
                    }
                    scans.Dispose();
                }
            }).ScheduleParallel();
        }

    }
}