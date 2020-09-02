using UnityEngine;
using Unity.Transforms;
using Unity.Burst;

using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Components.MovementSystem;
using InfluenceMap;
using CharacterAlignmentSystem;
using IAUS.Core;
using SpawnerSystem.ScriptableObjects;


namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateState))]
    public class PatrolAction : SystemBase
    {

        public EntityQueryDesc StaticObjectQuery = new EntityQueryDesc() { All = new ComponentType[] { typeof(Influencer) },
        
        Any = new ComponentType[]{typeof(Cover) }
        };

        public EntityQueryDesc DynamicAttackaleObjectQuery = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Influencer), typeof(CharacterAlignment) }
        };
        private EntityQuery _patrolUpdatesQuery;
        private EntityQuery _squadMemberUpdateQuery;
        private EntityQuery _PatrolActionQuery;
        EntityCommandBufferSystem _entityCommandBufferSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _patrolUpdatesQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(LocalToWorld)), 
                    ComponentType.ReadOnly(typeof(BaseAI)),ComponentType.ReadWrite(typeof(PatrolBuffer)) }
            });
            _squadMemberUpdateQuery = GetEntityQuery( new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(PatrolBuffer)), ComponentType.ReadWrite(typeof(SquadMemberBuffer))
                ,ComponentType.ReadOnly(typeof(LeaderTag))
                }
            });
            _PatrolActionQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(PatrolActionTag)), ComponentType.ReadWrite(typeof(Movement)),
                ComponentType.ReadWrite(typeof(InfluenceValues)), ComponentType.ReadWrite(typeof(PatrolBuffer)),ComponentType.ReadOnly(typeof(BaseAI))}

            });
        }
        protected override void  OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            float DT = Time.DeltaTime;
          //  EntityCommandBuffer.Concurrent entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

            systemDeps = new UpdatePatrol()
            {
                PositionsChunk=GetArchetypeChunkComponentType<LocalToWorld>(false),
                PatrolBufferChunk= GetArchetypeChunkBufferType<PatrolBuffer>(false),
                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(false)
            }.Schedule(_patrolUpdatesQuery, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps.Complete();
            systemDeps = new UpdateSquadMembersJobs()
           {
               PatrolChunk = GetArchetypeChunkComponentType<Patrol>(false),
               PatrolBufferChunk = GetArchetypeChunkBufferType<PatrolBuffer>(false),
               SquadBufferChunk = GetArchetypeChunkBufferType<SquadMemberBuffer>(false),
               getpoint =  GetComponentDataFromEntity<getpointTag>(true),
               follow = GetComponentDataFromEntity<FollowCharacter>(false),
            entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
           }.ScheduleParallel(_squadMemberUpdateQuery,systemDeps);
         
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);



            systemDeps = new PatrolActionJob() 
            { 
                EntityChunk = GetArchetypeChunkEntityType(),
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(false),
                InfluValuesChunk = GetArchetypeChunkComponentType<InfluenceValues>(false),
                  MoveChunk = GetArchetypeChunkComponentType<Movement>(false),
               PatrolBufferChunk = GetArchetypeChunkBufferType<PatrolBuffer>(false)


            }.ScheduleParallel(_PatrolActionQuery ,systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);


            systemDeps = new FollowStatusUpdate()
            {
                SquadBufferChunk = GetArchetypeChunkBufferType<SquadMemberBuffer>(false),
                Follow = GetComponentDataFromEntity<FollowCharacter>(false),
                  MoveChunk = GetArchetypeChunkComponentType<Movement>(false),

            }.ScheduleParallel(_patrolUpdatesQuery, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;

          
        }
    }
    [BurstCompile]
    public struct UpdatePatrol : IJobChunk
    {

        public ArchetypeChunkComponentType<Patrol> PatrolChunk;
        public ArchetypeChunkBufferType<PatrolBuffer> PatrolBufferChunk;
        public ArchetypeChunkComponentType<LocalToWorld> PositionsChunk;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
            NativeArray<LocalToWorld> Positions = chunk.GetNativeArray(PositionsChunk);
            //Possible need to change to getbufferfromEntity as bufferAccessor is readonly
            BufferAccessor<PatrolBuffer> bufferAccessor = chunk.GetBufferAccessor(PatrolBufferChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                Patrol patrol = patrols[i];
                DynamicBuffer<PatrolBuffer> buffer = bufferAccessor[i];
                LocalToWorld toWorld = Positions[i];

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

                patrols[i] = patrol;

            }
        }

    }
    [BurstCompile]
    public struct UpdateSquadMembersJobs : IJobChunk
    {
        public ArchetypeChunkBufferType<SquadMemberBuffer> SquadBufferChunk;
        public ArchetypeChunkBufferType<PatrolBuffer> PatrolBufferChunk;
        public ArchetypeChunkComponentType<Patrol> PatrolChunk;
       [ReadOnly] public ComponentDataFromEntity<getpointTag> getpoint;
        [NativeDisableParallelForRestriction]public ComponentDataFromEntity<FollowCharacter> follow;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
            BufferAccessor<PatrolBuffer> patrolbufferAccessor = chunk.GetBufferAccessor(PatrolBufferChunk);
            BufferAccessor<SquadMemberBuffer> squadBufferAccessor = chunk.GetBufferAccessor(SquadBufferChunk);

            for (int j = 0; j < chunk.Count; j++)
            {
                Patrol patrol = patrols[j];
                DynamicBuffer<PatrolBuffer> buffer = patrolbufferAccessor[j];
                DynamicBuffer<SquadMemberBuffer> Buffer = squadBufferAccessor[j];
                if (patrol.Status == ActionStatus.Idle || patrol.Status == ActionStatus.CoolDown)
                    if (patrol.LeaderUpdate)
                    {
                        for (int i = 0; i < Buffer.Length; i++)
                        {
                            Entity temp = Buffer[i].SquadMember;
                            if (!getpoint.Exists(temp))
                                entityCommandBuffer.AddComponent<getpointTag>(chunkIndex, temp);
                          FollowCharacter  tempFollow = follow[Buffer[i].SquadMember] ;
                            if (patrol.Status==ActionStatus.Running)
                                tempFollow.IsTargetMoving = true;
                            else
                                tempFollow.IsTargetMoving = false;

                            follow[Buffer[i].SquadMember] = tempFollow;

                        }
                        patrol.LeaderUpdate = false;

                    }
                patrols[j] = patrol;
            }
        }
    }
    [BurstCompile]
    public struct PatrolActionJob : IJobChunk
    {
        public ArchetypeChunkBufferType<PatrolBuffer> PatrolBufferChunk;
        public ArchetypeChunkComponentType<Patrol> PatrolChunk;
        public ArchetypeChunkComponentType<Movement> MoveChunk;
        public ArchetypeChunkComponentType<InfluenceValues> InfluValuesChunk;
        [ReadOnly]public ArchetypeChunkEntityType EntityChunk;
        public EntityCommandBuffer.Concurrent entityCommandBuffer;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
            NativeArray<Movement> movements = chunk.GetNativeArray(MoveChunk);
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            NativeArray<InfluenceValues> influences = chunk.GetNativeArray(InfluValuesChunk);
            BufferAccessor<PatrolBuffer> patrolbufferAccessor = chunk.GetBufferAccessor(PatrolBufferChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                Patrol patrol = patrols[i];
                Entity entity = entities[i];
                Movement move = movements[i];
                InfluenceValues InfluValues = influences[i];
                DynamicBuffer<PatrolBuffer> buffer = patrolbufferAccessor[i];

                //start
                if (patrol.Status == ActionStatus.Success)
                {
                    entityCommandBuffer.RemoveComponent<PatrolActionTag>(chunkIndex, entity);

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
                patrols[i] = patrol;
                movements[i] = move;
                influences[i] = InfluValues;
            }
        }
    }
    [BurstCompile]
    public struct FollowStatusUpdate : IJobChunk
    {
       [NativeDisableParallelForRestriction] public ComponentDataFromEntity<FollowCharacter> Follow;
        public ArchetypeChunkBufferType<SquadMemberBuffer> SquadBufferChunk;
        public ArchetypeChunkComponentType<Movement> MoveChunk;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            BufferAccessor<SquadMemberBuffer> squadBufferAccessor = chunk.GetBufferAccessor(SquadBufferChunk);
            NativeArray<Movement> movements = chunk.GetNativeArray(MoveChunk);
            for (int j = 0; j < chunk.Count; j++)
            {
                DynamicBuffer<SquadMemberBuffer> Buffer = squadBufferAccessor[j];
                Movement move = movements[j];
                for (int i = 0; i < Buffer.Length; i++)
                {
                   

                    FollowCharacter tempFollow = Follow[Buffer[i].SquadMember];
                    if (!move.Completed)
                        tempFollow.IsTargetMoving = true;
                    else
                        tempFollow.IsTargetMoving = false;

                    Follow[Buffer[i].SquadMember] = tempFollow;
                }
            }
        }
    }
}