//using UnityEngine;
//using Unity.Transforms;
//using Unity.Burst;
//using Unity.Jobs;
//using Unity.Entities;
//using Unity.Collections;
//using Components.MovementSystem;
//using InfluenceMap;
//using SpawnerSystem.ScriptableObjects;
//using IAUS.Core;

//namespace IAUS.ECS2
//{
//        [UpdateInGroup(typeof(IAUS_UpdateState))]

//    public class PatrolAction : SystemBase
//    {
//        private EntityQuery _patrolUpdatesQuery;
//        private EntityQuery _patrolStartQuery;

//        private EntityQuery _patrolUpdatesQueryLeader;
//        private EntityQuery _PatrolActionQuery;
//        EntityCommandBufferSystem _entityCommandBufferSystem;

//        protected override void OnCreate()
//        {
//            base.OnCreate();
//            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
//            _patrolUpdatesQuery = GetEntityQuery(new EntityQueryDesc()
//            {
//                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(LocalToWorld)),
//                    ComponentType.ReadOnly(typeof(BaseAI)),ComponentType.ReadWrite(typeof(PatrolBuffer)) },

//            });
//            _patrolStartQuery = GetEntityQuery(new EntityQueryDesc()
//            {
//                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(LocalToWorld)),
//                    ComponentType.ReadOnly(typeof(BaseAI)),ComponentType.ReadOnly(typeof(PatrolUpdateTag))
//                    ,ComponentType.ReadOnly(typeof(PatrolActionTag))
//                },

//            });
//            _patrolUpdatesQueryLeader = GetEntityQuery(new EntityQueryDesc()
//            {
//                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(LocalToWorld)),
//                    ComponentType.ReadOnly(typeof(BaseAI)),ComponentType.ReadWrite(typeof(PatrolBuffer)),ComponentType.ReadOnly(typeof(PatrolUpdateTag)) 
//                    ,ComponentType.ReadWrite(typeof(LeaderTag)) }

//            });
//            _PatrolActionQuery = GetEntityQuery(new EntityQueryDesc()
//            {
//                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadWrite(typeof(PatrolActionTag)), ComponentType.ReadWrite(typeof(Movement)),
//               ComponentType.ReadOnly(typeof(BaseAI))}

//            });
//        }
//        protected override void OnUpdate()
//        {

//            JobHandle systemDeps = Dependency;
//            //  EntityCommandBuffer.Concurrent entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

//            systemDeps = new UpdatePatrol()
//            {
//                PositionsChunk = GetArchetypeChunkComponentType<LocalToWorld>(false),
//                PatrolBufferChunk = GetArchetypeChunkBufferType<PatrolBuffer>(false),
//                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(false),
//                moveChunk =GetArchetypeChunkComponentType<Movement>(false)
//            }.Schedule(_patrolUpdatesQuery, systemDeps);
//            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

//            systemDeps = new StartPatrol() { 
//                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(false),
//                MoveChunk = GetArchetypeChunkComponentType<Movement>(false),
//                EntityChunk = GetArchetypeChunkEntityType(),
//                InfluValuesChunk = GetArchetypeChunkComponentType<InfluenceValues>(false),
//                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
//            }.Schedule(_patrolStartQuery, systemDeps);

//            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

//            systemDeps = new PatrolActionJob()
//            {
//                EntityChunk = GetArchetypeChunkEntityType(),
//                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
//                PatrolChunk = GetArchetypeChunkComponentType<Patrol>(false),
//                MoveChunk = GetArchetypeChunkComponentType<Movement>(false),
//            }.ScheduleParallel(_PatrolActionQuery, systemDeps);
//            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);


//            Dependency = systemDeps;
//        }


//        public struct UpdatePatrol : IJobChunk
//        {

//            public ArchetypeChunkComponentType<Patrol> PatrolChunk;
//            public ArchetypeChunkComponentType<Movement> moveChunk;

//            public ArchetypeChunkBufferType<PatrolBuffer> PatrolBufferChunk;
//            public ArchetypeChunkComponentType<LocalToWorld> PositionsChunk;
//            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//            {
//                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
//                NativeArray<Movement> movers = chunk.GetNativeArray(moveChunk);

//                NativeArray<LocalToWorld> Positions = chunk.GetNativeArray(PositionsChunk);
//                //Possible need to change to getbufferfromEntity as bufferAccessor is readonly
//                BufferAccessor<PatrolBuffer> bufferAccessor = chunk.GetBufferAccessor(PatrolBufferChunk);
//                for (int i = 0; i < chunk.Count; i++)
//                {
//                    Patrol patrol = patrols[i];
//                    Movement mover = movers[i];
//                    DynamicBuffer<PatrolBuffer> buffer = bufferAccessor[i];
//                    LocalToWorld toWorld = Positions[i];

//                        if (patrol.UpdatePatrolPoints)
//                        {
//                            buffer.Clear();
//                            patrol.MaxNumWayPoint = buffer.Length;
//                        }
//                    //if (patrol.UpdatePosition) 
//                    //{
//                        if (patrol.index >= buffer.Length)
//                            patrol.index = 0;

//                        patrol.DistanceAtStart = Vector3.Distance(toWorld.Position, buffer[patrol.index].WayPoint.Point);
//                        patrol.waypointRef= mover.TargetLocation = buffer[patrol.index].WayPoint.Point;
//                        patrols[i] = patrol;
//                    movers[i] = mover;
//                    // }
//                }
//            }

//        }


      

//        [BurstCompile]
//        public struct StartPatrol : IJobChunk
//        {
//            public ArchetypeChunkComponentType<Patrol> PatrolChunk;
//            public ArchetypeChunkComponentType<Movement> MoveChunk;
//            public ArchetypeChunkComponentType<InfluenceValues> InfluValuesChunk;
//            [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
//            public EntityCommandBuffer.Concurrent entityCommandBuffer;
//            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//            {
//                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
//                NativeArray<Movement> movements = chunk.GetNativeArray(MoveChunk);
//                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
//                NativeArray<InfluenceValues> influences = chunk.GetNativeArray(InfluValuesChunk);

//                for (int i = 0; i < chunk.Count; i++)
//                {
//                    Patrol patrol = patrols[i];
//                    Entity entity = entities[i];
//                    Movement move = movements[i];
//                    InfluenceValues InfluValues = influences[i];
//                    //Running

//                    InfluValues.TargetLocation = move.TargetLocation = patrol.waypointRef;
//                        move.SetTargetLocation = true;
//                        patrol.Status = ActionStatus.Running;
//                        move.CanMove = true;
//                    entityCommandBuffer.RemoveComponent<PatrolUpdateTag>(chunkIndex, entity);

//                    patrols[i] = patrol;
//                    movements[i] = move;
//                   influences[i] = InfluValues;
//                }
//            }
//        }



//        [BurstCompile]
//        public struct PatrolActionJob : IJobChunk
//        {
//            public ArchetypeChunkComponentType<Patrol> PatrolChunk;
//            public ArchetypeChunkComponentType<Movement> MoveChunk;
//            [ReadOnly] public ArchetypeChunkEntityType EntityChunk;
//            public EntityCommandBuffer.Concurrent entityCommandBuffer;
//            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//            {
//                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
//                NativeArray<Movement> movements = chunk.GetNativeArray(MoveChunk);
//                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);

//                for (int i = 0; i < chunk.Count; i++)
//                {
//                    Patrol patrol = patrols[i];
//                    Entity entity = entities[i];
//                    Movement move = movements[i];

//                    if (patrol.Status == ActionStatus.Success)
//                    {
//                        entityCommandBuffer.RemoveComponent<PatrolActionTag>(chunkIndex, entity);

//                        return;
//                    }

//                    //complete
                  
//                        if (move.Completed )
//                        {
//                            Debug.Log("are you being called");
//                            patrol.Status = ActionStatus.Success;

//                        }
                  
//                    //start
            

        

//                    // move to independent system. 

//                    if (move.TargetLocationCrowded)
//                    {
//                        patrol.index++;
//                        if (patrol.index >= patrol.MaxNumWayPoint)
//                            patrol.index = 0;
//                        entityCommandBuffer.AddComponent<PatrolUpdateTag>(chunkIndex, entity);
//                        patrol.Status = ActionStatus.Running;
//                        move.TargetLocationCrowded = false;

//                    }


//                    patrols[i] = patrol;
//                    movements[i] = move;
//                }
//            }
//        }

//    }
//}