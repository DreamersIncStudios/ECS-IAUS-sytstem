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
using DreamersInc.FactionSystem;


namespace IAUS.ECS.Systems
{
    public class UpdatePatrol : SystemBase
    {
        private EntityQuery DistanceCheck;
        private EntityQuery PatrolScore;
        private EntityQuery BufferPatrol;
        private EntityQuery CompleteCheck;

        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            DistanceCheck = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadOnly(typeof(LocalToWorld)) }
            });
            DistanceCheck.SetChangedVersionFilter(
                new ComponentType[] { 
                    ComponentType.ReadOnly(typeof(LocalToWorld)),
                    ComponentType.ReadWrite(typeof(Patrol))
                });
            PatrolScore = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol)), ComponentType.ReadOnly(typeof(CharacterStatComponent)), ComponentType.ReadOnly(typeof(IAUSBrain)),
                                    ComponentType.ReadOnly(typeof(AlertLevel))
                }
            });
            BufferPatrol = GetEntityQuery(new EntityQueryDesc() {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(IAUSBrain)), ComponentType.ReadWrite(typeof(PatrolWaypointBuffer))}
            });
                
           CompleteCheck = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Patrol)), ComponentType.ReadOnly(typeof(PatrolActionTag)) }
            });

            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new GetDistanceToNextPoint()
            {
                PatrolChunk = GetComponentTypeHandle<Patrol>(false),
                TransformChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }.ScheduleParallel(DistanceCheck, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new CheckThreatAtWaypoint() { 
                IAUSBrainChunk = GetComponentTypeHandle<IAUSBrain>(true),
                PatrolBuffer = GetBufferTypeHandle<PatrolWaypointBuffer>(false),
                PatrolChunk = GetComponentTypeHandle<Patrol>(false)
            }.ScheduleParallel(BufferPatrol, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new ScoreState() 
            {
                PatrolChunk = GetComponentTypeHandle<Patrol>(false),
                StatsChunk = GetComponentTypeHandle<CharacterStatComponent>(true)
            }.ScheduleParallel(PatrolScore, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new CompletionChecker()
            {
                PatrolChunk = GetComponentTypeHandle<Patrol>(false),
                Buffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                EntityChunk = GetEntityTypeHandle()
            }.Schedule(CompleteCheck, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        
        }

        [BurstCompile]
        public struct GetDistanceToNextPoint : IJobChunk
        {
            public ComponentTypeHandle<Patrol> PatrolChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> TransformChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
                NativeArray<LocalToWorld> toWorlds = chunk.GetNativeArray(TransformChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Patrol patrol = patrols[i];
                    patrol.distanceToPoint = Vector3.Distance(patrol.CurWaypoint.Position, toWorlds[i].Position);
                    if (patrol.Complete)
                        patrol.distanceToPoint = 0.0f;
                    patrols[i] = patrol;
                }

            }
        }

        [BurstCompile]
        public struct ScoreState : IJobChunk
        {
            public ComponentTypeHandle<Patrol> PatrolChunk;
            [ReadOnly]public ComponentTypeHandle<CharacterStatComponent> StatsChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);

                NativeArray<CharacterStatComponent> Stats = chunk.GetNativeArray(StatsChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Patrol patrol = patrols[i];
                    if (patrol.stateRef.IsCreated)
                    {
                        float healthRatio = Stats[i].HealthRatio;
                        float TotalScore = patrol.DistanceToPoint.Output(patrol.DistanceRatio) * patrol.HealthRatio.Output(healthRatio);
                        patrol.TotalScore = Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * patrol.mod) * TotalScore);
                    }
                    patrols[i] = patrol;
                }
            }
        }
      //  [BurstCompile]
        public struct CheckThreatAtWaypoint : IJobChunk
        {
            public BufferTypeHandle<PatrolWaypointBuffer> PatrolBuffer;
            [ReadOnly]public ComponentTypeHandle<IAUSBrain> IAUSBrainChunk;
            public ComponentTypeHandle<Patrol> PatrolChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                BufferAccessor<PatrolWaypointBuffer> bufferAccess = chunk.GetBufferAccessor(PatrolBuffer);
                NativeArray<IAUSBrain> Brains = chunk.GetNativeArray(IAUSBrainChunk);
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    DynamicBuffer<PatrolWaypointBuffer> buffer = bufferAccess[i];
                    Patrol patrol = patrols[i];

                    for (int j = 0; j < buffer.Length; j++)
                    {
                        PatrolWaypointBuffer point = buffer[j];
                        point.WayPoint.InfluenceAtPosition = InfluenceGridMaster.Instance.grid.GetGridObject(point.WayPoint.Position).GetValueNormalized(FactionManager.Database.GetFaction( Brains[i].factionID));
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
        public struct CompletionChecker : IJobChunk
        {
            public ComponentTypeHandle<Patrol> PatrolChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public EntityCommandBuffer.ParallelWriter Buffer;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Patrol patrol = patrols[i];
                    if (patrol.Complete)
                    {
                        Buffer.RemoveComponent<PatrolActionTag>(chunkIndex, entities[i]);
                        patrol.Status = ActionStatus.Success;

                        patrols[i] = patrol;
                    }
                }
            }
        }
    }
}
