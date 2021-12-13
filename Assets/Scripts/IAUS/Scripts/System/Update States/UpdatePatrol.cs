using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
using Stats;
using AISenses;
using DreamersInc.InflunceMapSystem;
using PixelCrushers.LoveHate;

namespace IAUS.ECS.Systems
{
    public class UpdatePatrol : SystemBase
    {
        private EntityQuery DistanceCheckPatrol;
        private EntityQuery DistanceCheckTraverse;

        private EntityQuery PatrolScore;
        private EntityQuery TraverseScore;

        private EntityQuery BufferPatrol;
        private EntityQuery CompleteCheckPatrol;
        private EntityQuery CompleteCheckTraverse;

        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            DistanceCheckPatrol = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadOnly(typeof(LocalToWorld)) }
            });
            DistanceCheckPatrol.SetChangedVersionFilter(
                new ComponentType[] { 
                    ComponentType.ReadOnly(typeof(LocalToWorld)),
                    ComponentType.ReadWrite(typeof(Patrol))
                });

            DistanceCheckTraverse = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Traverse)), ComponentType.ReadOnly(typeof(LocalToWorld)) }
            });
            DistanceCheckTraverse.SetChangedVersionFilter(
                new ComponentType[] {
                    ComponentType.ReadOnly(typeof(LocalToWorld)),
                    ComponentType.ReadWrite(typeof(Traverse))
                });

            PatrolScore = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadOnly(typeof(EnemyStats)), ComponentType.ReadOnly(typeof(IAUSBrain)),
                                    ComponentType.ReadOnly(typeof(AlertLevel))
                }
            });

            //Todo Remove EnemyStats reference
            PatrolScore = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadOnly(typeof(EnemyStats)), ComponentType.ReadOnly(typeof(IAUSBrain)),
                                    ComponentType.ReadOnly(typeof(AttackTargetState))
                }
            });
            TraverseScore = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Traverse)), ComponentType.ReadOnly(typeof(EnemyStats)), ComponentType.ReadOnly(typeof(IAUSBrain)),
                                    ComponentType.ReadOnly(typeof(AlertLevel))
                }
            });
            BufferPatrol = GetEntityQuery(new EntityQueryDesc() {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(IAUSBrain)), ComponentType.ReadWrite(typeof(TravelWaypointBuffer))}
            });
                
           CompleteCheckPatrol = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Patrol)), ComponentType.ReadOnly(typeof(PatrolActionTag)) }
            });
            CompleteCheckTraverse = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Traverse)), ComponentType.ReadOnly(typeof(TraverseActionTag)) }
            });

            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new GetDistanceToNextPoint<Patrol>()
            {
                MoveChunk = GetComponentTypeHandle<Patrol>(false),
                TransformChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }.ScheduleParallel(DistanceCheckPatrol, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new GetDistanceToNextPoint<Traverse>()
            {
                MoveChunk = GetComponentTypeHandle<Traverse>(false),
                TransformChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }.ScheduleParallel(DistanceCheckTraverse, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            //TODO Debug performance here 
            //systemDeps = new CheckThreatAtWaypoint() { 
            //    IAUSBrainChunk = GetComponentTypeHandle<IAUSBrain>(true),
            //    PatrolBuffer = GetBufferTypeHandle<PatrolWaypointBuffer>(false),
            //    PatrolChunk = GetComponentTypeHandle<Patrol>(false)
            //}.ScheduleParallel(BufferPatrol, systemDeps);

            //_entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new ScoreStatePatrol() 
            {
                PatrolChunk = GetComponentTypeHandle<Patrol>(false),
                AttackChunk = GetComponentTypeHandle<AttackTargetState>(true),
                StatsChunk = GetComponentTypeHandle<EnemyStats>(true)
            }.ScheduleParallel(PatrolScore, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            
            systemDeps = new ScoreStateTraverse()
            {
                TraverseChunk = GetComponentTypeHandle<Traverse>(false),
                StatsChunk = GetComponentTypeHandle<EnemyStats>(true)
            }.ScheduleParallel(TraverseScore, systemDeps);
            
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);


            systemDeps = new CompletionChecker<Patrol, PatrolActionTag>()
            {
                MoveChunk = GetComponentTypeHandle<Patrol>(false),
                Buffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                EntityChunk = GetEntityTypeHandle()
            }.Schedule(CompleteCheckPatrol, systemDeps);


            systemDeps = new CompletionChecker<Traverse, TraverseActionTag>()
            {
                MoveChunk = GetComponentTypeHandle<Traverse>(false),
                Buffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                EntityChunk = GetEntityTypeHandle()
            }.Schedule(CompleteCheckTraverse, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        
        }

        [BurstCompile]
        public struct GetDistanceToNextPoint<T> : IJobChunk
            where T : unmanaged, MovementState
        {
            public ComponentTypeHandle<T> MoveChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> TransformChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<T> MovesStyles = chunk.GetNativeArray(MoveChunk);
                NativeArray<LocalToWorld> toWorlds = chunk.GetNativeArray(TransformChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    T patrol = MovesStyles[i];
                    patrol.distanceToPoint = Vector3.Distance(patrol.CurWaypoint.Position, toWorlds[i].Position);
                    if (patrol.Complete)
                        patrol.distanceToPoint = 0.0f;
                    MovesStyles[i] = patrol;
                }

            }
        }

         [BurstCompile]
        public struct ScoreStatePatrol: IJobChunk
        {
            public ComponentTypeHandle<Patrol> PatrolChunk;
            [ReadOnly]public ComponentTypeHandle<AttackTargetState> AttackChunk;
            [ReadOnly]public ComponentTypeHandle<EnemyStats> StatsChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
                NativeArray<AttackTargetState> attacks = chunk.GetNativeArray(AttackChunk);
                NativeArray<EnemyStats> Stats = chunk.GetNativeArray(StatsChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Patrol patrol = patrols[i];
                    if (patrol.stateRef.IsCreated)
                    {
                        float attackRatio = attacks[i].HighScoreAttack.AttackTarget.entity == Entity.Null  ? 1.0f : attacks[i].HighScoreAttack.AttackDistanceRatio;
                        float healthRatio = Stats[i].HealthRatio;
                        float TotalScore = patrol.DistanceToPoint.Output(patrol.DistanceRatio) * patrol.HealthRatio.Output(healthRatio) * patrol.TargetInRange.Output(attackRatio);
                        patrol.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * patrol.mod) * TotalScore);
                    }
                    patrols[i] = patrol;
                }
            }
        }

        [BurstCompile]
        public struct ScoreStateTraverse : IJobChunk
        {
            public ComponentTypeHandle<Traverse> TraverseChunk;
            [ReadOnly] public ComponentTypeHandle<EnemyStats> StatsChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Traverse> traverses = chunk.GetNativeArray(TraverseChunk);

                NativeArray<EnemyStats> Stats = chunk.GetNativeArray(StatsChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Traverse traverse = traverses[i];
                    if (traverse.stateRef.IsCreated)
                    {
                        float healthRatio = Stats[i].HealthRatio;
                        float TotalScore = traverse.DistanceToPoint.Output(traverse.DistanceRatio) * traverse.HealthRatio.Output(healthRatio);
                        traverse.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * traverse.mod) * TotalScore);
                    }
                    traverses[i] = traverse;
                }
            }
        }

        //  [BurstCompile]
        public struct CheckThreatAtWaypoint<T> : IJobChunk
            where T : unmanaged, MovementState

        {
            public BufferTypeHandle<TravelWaypointBuffer> PatrolBuffer;
            [ReadOnly]public ComponentTypeHandle<IAUSBrain> IAUSBrainChunk;
            public ComponentTypeHandle<Patrol> PatrolChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<TravelWaypointBuffer> bufferAccess = chunk.GetBufferAccessor(PatrolBuffer);
                NativeArray<IAUSBrain> Brains = chunk.GetNativeArray(IAUSBrainChunk);
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    DynamicBuffer<TravelWaypointBuffer> buffer = bufferAccess[i];
                    Patrol patrol = patrols[i];

                    for (int j = 0; j < buffer.Length; j++)
                    {
                        TravelWaypointBuffer point = buffer[j];
                        point.WayPoint.InfluenceAtPosition = InfluenceGridMaster.Instance.grid.GetGridObject(point.WayPoint.Position).GetValueNormalized(LoveHate.factionDatabase.GetFaction( Brains[i].factionID));
                        buffer[j] = point;
                        if (j == patrol.WaypointIndex) {
                            patrol.CurWaypoint = point.WayPoint;
                        }
                    }
                    
                    patrols[i] = patrol;

                }
            }
        }

        [BurstCompile]
        public struct CompletionChecker<T,A> : IJobChunk
            where T : unmanaged, MovementState
            where A : unmanaged, IComponentData

        {
            public ComponentTypeHandle<T> MoveChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public EntityCommandBuffer.ParallelWriter Buffer;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<T> Moves = chunk.GetNativeArray(MoveChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    T move = Moves[i];
                    if (move.Complete)
                    {
                        Buffer.RemoveComponent<A>(chunkIndex, entities[i]);
                        move.Status = ActionStatus.Success;

                        Moves[i] = move;
                    }
                }
            }
        }
    }
}
