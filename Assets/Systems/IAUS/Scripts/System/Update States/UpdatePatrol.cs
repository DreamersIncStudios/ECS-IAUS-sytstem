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

        private EntityQuery PatrolScore;

        private EntityQuery BufferPatrol;
        private EntityQuery CompleteCheckPatrol;

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

            BufferPatrol = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(IAUSBrain)), ComponentType.ReadWrite(typeof(TravelWaypointBuffer)) }
            });

            CompleteCheckPatrol = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Patrol)), ComponentType.ReadOnly(typeof(PatrolActionTag)) }
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

            systemDeps = new CompletionChecker<Patrol, PatrolActionTag>()
            {
                MoveChunk = GetComponentTypeHandle<Patrol>(false),
                Buffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
                EntityChunk = GetEntityTypeHandle(),
                WaypointChunk = GetBufferTypeHandle<TravelWaypointBuffer>(true),
                WaitChunk = GetComponentTypeHandle<Wait>(false)

            }.Schedule(CompleteCheckPatrol, systemDeps);



            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;

        }



        [BurstCompile]
        public struct ScoreStatePatrol : IJobChunk
        {
            public ComponentTypeHandle<Patrol> PatrolChunk;
            [ReadOnly] public ComponentTypeHandle<AttackTargetState> AttackChunk;
            [ReadOnly] public ComponentTypeHandle<EnemyStats> StatsChunk;

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
                        float attackRatio = attacks[i].HighScoreAttack.AttackTarget.entity == Entity.Null ? 1.0f :attacks[i].HighScoreAttack.AttackDistanceRatio;

                        float healthRatio = Stats[i].HealthRatio;
                        float TotalScore = patrol.DistanceToPoint.Output(patrol.DistanceRatio) * patrol.HealthRatio.Output(healthRatio) * patrol.TargetInRange.Output(attackRatio);
                        patrol.TotalScore = patrol.Status != ActionStatus.CoolDown ? Mathf.Clamp01(TotalScore + ((1.0f - TotalScore) * patrol.mod) * TotalScore) : 0.0f;
                    }
                    patrols[i] = patrol;
                }
            }
        }




    }
}
