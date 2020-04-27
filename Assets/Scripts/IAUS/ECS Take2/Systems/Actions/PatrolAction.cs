using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS.Component;
using InfluenceMap;
using InfluenceMap.Factions;
using IAUS.Core;
namespace IAUS.ECS2
{
    [UpdateAfter(typeof(StateScoreSystem))]
    [UpdateInGroup(typeof(IAUS_UpdateState))]
    public class PatrolAction : JobComponentSystem
    {

        public EntityQueryDesc StaticObjectQuery = new EntityQueryDesc() { All = new ComponentType[] { typeof(Influencer) },
        
        Any = new ComponentType[]{typeof(Cover) }
        };

        public EntityQueryDesc DynamicAttackaleObjectQuery = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Influencer), typeof(Attackable) }
        };

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float DT = Time.DeltaTime;
            JobHandle PatrolPointUpdate = Entities.ForEach((ref Patrol patrol,
            ref DynamicBuffer<PatrolBuffer> buffer, ref LocalToWorld toWorld
             ) =>
            {
                if (patrol.UpdatePatrolPoints)
                {
                    buffer.Clear();



                    patrol.MaxNumWayPoint = buffer.Length;
                }

                if (patrol.Status == ActionStatus.Idle || patrol.Status == ActionStatus.CoolDown)
                    if (patrol.UpdatePostition)
                    {
                        if (patrol.index >= buffer.Length)
                            patrol.index = 0;
                        patrol.DistanceAtStart = Vector3.Distance(toWorld.Position, buffer[patrol.index].WayPoint.Point);
                        patrol.UpdatePostition = false;
                    }

            }).Schedule(inputDeps);



            JobHandle PatrolAction = Entities.ForEach((ref PatrolActionTag PatrolTag, ref Patrol patrol, ref Movement move,
                ref DynamicBuffer<PatrolBuffer> buffer, ref InfluenceValues InfluValues
                ) =>
            {
                //start
                if (patrol.Status == ActionStatus.Success)
                    return;
                if (patrol.UpdatePostition)
                    return;

                //Running
                if (!buffer[patrol.index].WayPoint.Point.Equals(move.TargetLocation))
                {
                    move.TargetLocation = buffer[patrol.index].WayPoint.Point;
                    InfluValues.TargetLocation = buffer[patrol.index].WayPoint.Point;
                    move.SetTargetLocation = true;
                    patrol.Status = ActionStatus.Running;
                    move.Completed = false;
                    move.CanMove = true;
                }
                if (move.DistanceRemaining <= 10.5f && move.DistanceRemaining >= 3.5)
                {

                    if (InfluValues.InfluenceAtTarget.Ally.Proximity.x > patrol.MaxInfluenceAtPoint)
                    {
                        patrol.index++;
                        if (patrol.index >= patrol.MaxNumWayPoint)
                            patrol.index = 0;

                        move.TargetLocation = buffer[patrol.index].WayPoint.Point;
                        move.SetTargetLocation = true;
                        InfluValues.TargetLocation = buffer[patrol.index].WayPoint.Point;
                        patrol.Status = ActionStatus.Running;
                    }
                }
                //complete
                if (patrol.Status == ActionStatus.Running)
                {
                    if (move.Completed && !move.CanMove)
                    {
                        patrol.Status = ActionStatus.Success;
                    }
                }

            }).Schedule(PatrolPointUpdate);

            

            return PatrolAction;
        }
    }

    public struct PatrolActionTag : IComponentData { }
}