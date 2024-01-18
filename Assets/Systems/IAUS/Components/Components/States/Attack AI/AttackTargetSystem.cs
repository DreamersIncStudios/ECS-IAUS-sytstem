using DreamersInc.InflunceMapSystem;
using IAUS.ECS.Component;
using Stats.Entities;
using Unity.Burst;
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

            new MeleeTargetJob() { Influence = GetComponentLookup<InfluenceComponent>() }.ScheduleParallel();
            new RangeTargetJob() { Influence = GetComponentLookup<InfluenceComponent>() }.ScheduleParallel();
            new MagicTargetJob() { Influence = GetComponentLookup<InfluenceComponent>() }.ScheduleParallel();
        }
[BurstCompile]
        partial struct MeleeTargetJob : IJobEntity
        {
             [ReadOnly][NativeDisableParallelForRestriction]public ComponentLookup<InfluenceComponent> Influence;
       
            void Execute(ref MeleeAttackSubState melee, ref DynamicBuffer<ScanPositionBuffer> buffer, ref Vision vision, in AttackState attackState
              ,in InfluenceComponent test)
            {
                if (buffer.Length == 0)
                    return;
                if (attackState.IsTargeting && buffer.Length+1 >= melee.AttackTargetIndex)
                {
                    melee.AttackTargetLocation = buffer[melee.AttackTargetIndex].target.LastKnownPosition;
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
                            //Todo check influence at location 
                            if (test.Threat > Influence[visibleTargetInArea[i].target.Entity].Protection)
                            {
                                indexOf = i;
                                break;
                            }
                        }

                        if (vision.EngageRadius > visibleTargetInArea[indexOf].dist)
                        {
                       //     melee.TargetEntity = visibleTargetInArea[indexOf].target.Entity;
                            melee.AttackTargetLocation = visibleTargetInArea[indexOf].target.LastKnownPosition;
                            melee.TargetInRange = true;
                            melee.TargetEntity = visibleTargetInArea[indexOf].target.Entity;
                        }
                    }
                    else
                    {
                        melee.TargetInRange = false;
                    }

                    scans.Dispose();
                }
            }
        }
        [BurstCompile]

         partial struct RangeTargetJob : IJobEntity
        {
             [ReadOnly][NativeDisableParallelForRestriction]public ComponentLookup<InfluenceComponent> Influence;
       
            void Execute(ref RangedAttackSubState range, ref DynamicBuffer<ScanPositionBuffer> buffer, ref Vision vision,
                in AttackState attackState, in InfluenceComponent test)
            {
                if (buffer.Length == 0)
                    return;
                if (attackState.IsTargeting && buffer.Length >= range.AttackTargetIndex)
                {
                    range.AttackTargetLocation = buffer[range.AttackTargetIndex].target.LastKnownPosition;
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
                            //Todo check influence at location 
                            if (test.Threat > Influence[visibleTargetInArea[i].target.Entity].Protection)
                            {
                                indexOf = i;
                                break;
                            }
                        }

                        if (vision.EngageRadius > visibleTargetInArea[indexOf].dist)
                        {
                            range.TargetEntity = visibleTargetInArea[indexOf].target.Entity;
                            range.AttackTargetLocation = visibleTargetInArea[indexOf].target.LastKnownPosition;
                            range.TargetInRange = true;
                        }
                    }
                    else
                    {
                        range.TargetInRange = false;
                    }

                    scans.Dispose();
                }
            }
        }
        [BurstCompile]

          partial struct MagicTargetJob : IJobEntity
        {
             [ReadOnly][NativeDisableParallelForRestriction]public ComponentLookup<InfluenceComponent> Influence;
       
            void Execute(ref MagicAttackSubState magic, ref DynamicBuffer<ScanPositionBuffer> buffer, ref Vision vision,
                in AttackState attackState, in InfluenceComponent test)
            {
                if (buffer.Length == 0)
                    return;
                if (attackState.IsTargeting && buffer.Length >= magic.AttackTargetIndex)
                {
                    magic.AttackTargetLocation = buffer[magic.AttackTargetIndex].target.LastKnownPosition;
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
                            //Todo check influence at location 
                            if (test.Threat > Influence[visibleTargetInArea[i].target.Entity].Protection)
                            {
                                indexOf = i;
                                break;
                            }
                        }

                        if (vision.EngageRadius > visibleTargetInArea[indexOf].dist)
                        {
                            magic.TargetEntity = visibleTargetInArea[indexOf].target.Entity;
                            magic.AttackTargetLocation = visibleTargetInArea[indexOf].target.LastKnownPosition;
                            magic.TargetInRange  = true;
                        }
                    }
                    else
                    {
                        magic.TargetInRange  = false;
                    }

                    scans.Dispose();
                }
            }
        }
          
    }
}