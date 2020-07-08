using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Components.MovementSystem;
using InfluenceMap;
using InfluenceMap.Factions;
using IAUS.Core;
using SpawnerSystem.ScriptableObjects;


namespace IAUS.ECS2
{
    [UpdateAfter(typeof(StateScoreSystem))]
    [UpdateInGroup(typeof(IAUS_UpdateState))]
    public class PatrolAction : SystemBase
    {

        public EntityQueryDesc StaticObjectQuery = new EntityQueryDesc() { All = new ComponentType[] { typeof(Influencer) },
        
        Any = new ComponentType[]{typeof(Cover) }
        };

        public EntityQueryDesc DynamicAttackaleObjectQuery = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Influencer), typeof(Attackable) }
        };
        EntityCommandBufferSystem _entityCommandBufferSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void  OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            float DT = Time.DeltaTime;
            EntityCommandBuffer.Concurrent entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

            systemDeps = Entities.ForEach((ref Patrol patrol,
            ref DynamicBuffer<PatrolBuffer> buffer, ref LocalToWorld toWorld, in BaseAI baseAi
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
                        patrol.waypointRef = buffer[patrol.index].WayPoint.Point;
                        patrol.UpdatePostition = false;

                        patrol.LeaderUpdate = true;
                    }

            }).Schedule(systemDeps);

            ComponentDataFromEntity<getpointTag> getpoint = GetComponentDataFromEntity<getpointTag>(true);
           systemDeps = Entities
                .WithNativeDisableParallelForRestriction(getpoint)
                .WithReadOnly(getpoint)
                .ForEach((int nativeThreadIndex, ref DynamicBuffer<SquadMemberBuffer> Buffer, ref DynamicBuffer<PatrolBuffer> buffer, ref Patrol patrol, in LeaderTag leader ) =>
             {
                 if (patrol.Status == ActionStatus.Idle || patrol.Status == ActionStatus.CoolDown)
                     if (patrol.LeaderUpdate)
                     {
                         for (int i = 0; i < Buffer.Length; i++)
                         {
                             Entity temp = Buffer[i].SquadMember;
                             if (!getpoint.Exists(temp))
                                 entityCommandBuffer.AddComponent<getpointTag>(nativeThreadIndex, temp);
                         }
                         patrol.LeaderUpdate = false;

                     }
             })

            .Schedule(systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);



            systemDeps = Entities.ForEach((Entity entity, int nativeThreadIndex, ref PatrolActionTag PatrolTag, ref Patrol patrol, ref Movement move,
                ref DynamicBuffer<PatrolBuffer> buffer, ref InfluenceValues InfluValues, in BaseAI baseAi
                ) =>
            {
                //start
                if (patrol.Status == ActionStatus.Success)
                {
                          entityCommandBuffer.RemoveComponent<PatrolActionTag>(nativeThreadIndex, entity);

                    return;
                }
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

                // move to independent system. 

                if (move.TargetLocationCrowded)
                {
                    patrol.index++;
                    if (patrol.index >= patrol.MaxNumWayPoint)
                        patrol.index = 0;

                    move.TargetLocation = buffer[patrol.index].WayPoint.Point;
                    move.SetTargetLocation = true;
                    InfluValues.TargetLocation = buffer[patrol.index].WayPoint.Point;
                    patrol.Status = ActionStatus.Running;
                    move.TargetLocationCrowded = false;

                }
                
                //complete
                if (patrol.Status == ActionStatus.Running)
                {
                    if (move.Completed && !move.CanMove)
                    {
                        patrol.Status = ActionStatus.Success;

                    }
                }


            }).Schedule(systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            ComponentDataFromEntity<FollowCharacter> follow = GetComponentDataFromEntity<FollowCharacter>(false);

           systemDeps = Entities
                .WithNativeDisableParallelForRestriction(follow)
                .ForEach((int nativeThreadIndex, ref DynamicBuffer<SquadMemberBuffer> Buffer, in Movement move, in Patrol patrol, in LeaderTag leader, in PatrolActionTag tag)
            =>
            {
                for (int i = 0; i < Buffer.Length; i++)
                {
                   FollowCharacter tempFollow = follow[Buffer[i].SquadMember];
                    if (!move.Completed)
                        tempFollow.IsTargetMoving = true;
                    else
                        tempFollow.IsTargetMoving = false;

                    follow[Buffer[i].SquadMember] = tempFollow;
                }

            }
            ).Schedule(systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;

          
        }
    }

 
}