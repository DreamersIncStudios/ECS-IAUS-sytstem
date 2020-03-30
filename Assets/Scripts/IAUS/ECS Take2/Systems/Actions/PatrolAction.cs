using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using IAUS.ECS.Component;
using InfluenceMap;

namespace IAUS.ECS2
{
    [UpdateAfter(typeof(StateScoreSystem))]
    
    public class PatrolAction : JobComponentSystem
    {

        public EntityQueryDesc StaticObjectQuery = new EntityQueryDesc() { All = new ComponentType[] { typeof(Influencer) },
        
        Any = new ComponentType[]{typeof(Cover) }
        };

        public EntityQueryDesc DynamicObjectQuery = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Influencer) },

            Any = new ComponentType[] { typeof(Attackable) }
        };

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            float DT = Time.DeltaTime;
            JobHandle PatrolPointUpdate = Entities.ForEach((ref Patrol patrol,
            ref DynamicBuffer<PatrolBuffer> buffer, ref LocalToWorld toWorld, ref PatrolActionTag tag
             ) =>
            {
                if (patrol.Status == ActionStatus.Idle || patrol.Status == ActionStatus.CoolDown)
                    if (patrol.UpdatePostition)
                    {
                        if (patrol.index >= buffer.Length)
                            patrol.index = 0;
                        patrol.DistanceAtStart = Vector3.Distance(toWorld.Position, buffer[patrol.index].WayPoint.Point);
                        patrol.UpdatePostition = false;
                    }

            }).Schedule(inputDeps);

         NativeArray<Entity> StaticObjects= GetEntityQuery(StaticObjectQuery).ToEntityArray(Allocator.TempJob);
            
            NativeArray<Entity> DynamicObjects = GetEntityQuery(DynamicObjectQuery).ToEntityArray(Allocator.TempJob);

            ComponentDataFromEntity<LocalToWorld> positions = GetComponentDataFromEntity<LocalToWorld>(true);
            ComponentDataFromEntity<Influencer> Influence = GetComponentDataFromEntity<Influencer>(true);


        JobHandle CheckInfluence = Entities.
                WithDeallocateOnJobCompletion(StaticObjects)
                .WithReadOnly(StaticObjects)
                .WithDeallocateOnJobCompletion(DynamicObjects)
                .WithReadOnly(DynamicObjects)
                .WithNativeDisableParallelForRestriction(positions)
                .WithReadOnly(positions)
                .WithNativeDisableParallelForRestriction(Influence)
                .WithReadOnly(Influence)
                
                .ForEach((DynamicBuffer<PatrolBuffer> buffer, ref Patrol patrol, ref PatrolActionTag tag) => 
            {

                // need to add a check to YZ vertial angle or do a raycast to see if below
                // possible make influnce a float 4 with distance in x y and z or vector displacement
                Gridpoint InfluenceAtPoint = new Gridpoint();
                for (int index = 0; index < StaticObjects.Length; index++)
                {
                    float dist = Vector3.Distance(buffer[patrol.index].WayPoint.Point, positions[StaticObjects[index]].Position);
                    Influencer Temp = Influence[StaticObjects[index]];
                    if (dist < Temp.influence.Proximity.y)
                    {
                        InfluenceAtPoint.Global.Proximity.x += Temp.influence.Proximity.x * (1 - dist/Temp.influence.Proximity.y);
                    }
                    if (dist < Temp.influence.Threat.y)
                    {
                        InfluenceAtPoint.Enemy.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                    }
                }
                for (int index = 0; index < DynamicObjects.Length; index++)
                {
                    float dist = Vector3.Distance(buffer[patrol.index].WayPoint.Point, positions[DynamicObjects[index]].Position);
                    Influencer Temp = Influence[DynamicObjects[index]];
                    if (dist < Temp.influence.Proximity.y)
                    {
                        InfluenceAtPoint.Enemy.Proximity.x += Temp.influence.Proximity.x * (1 - dist / Temp.influence.Proximity.y);
                    }
                    if (dist < Temp.influence.Threat.y)
                    {
                        InfluenceAtPoint.Enemy.Threat.x += Temp.influence.Threat.x * (1 - dist / Temp.influence.Threat.y);
                    }
                }

                //  Debug.Log(InfluenceAtPoint.Global.Proximity.x);

            })
                .Schedule(PatrolPointUpdate);




            JobHandle PatrolAction = Entities.ForEach((ref PatrolActionTag PatrolTag, ref Patrol patrol, ref Movement move,
                ref DynamicBuffer<PatrolBuffer> buffer
                ) => {
                //start
                if (patrol.Status == ActionStatus.Success)
                    return;
                    if (patrol.UpdatePostition)
                        return;
                        //Running
                        if (!buffer[patrol.index].WayPoint.Point.Equals( move.TargetLocation)) 
                {
                    move.TargetLocation = buffer[patrol.index].WayPoint.Point;
                    patrol.Status = ActionStatus.Running;
                    move.Completed = false;
                    move.CanMove = true;
                }
                    //complete
                if (patrol.Status == ActionStatus.Running)
                {
                    if (move.Completed && !move.CanMove)
                    {
                        patrol.Status = ActionStatus.Success;

                    }
                }

            }).Schedule(CheckInfluence);

            

            return PatrolAction;
        }
    }

    public struct PatrolActionTag : IComponentData { }
}