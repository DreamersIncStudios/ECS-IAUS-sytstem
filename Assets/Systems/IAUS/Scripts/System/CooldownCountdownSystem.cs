using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using IAUS.ECS.Component;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
namespace IAUS.ECS.Systems
{
    public partial class CooldownCountdownSystem : SystemBase
    {
        private EntityQuery PatrolCooldown;
        private EntityQuery TraverseCooldown;

        private EntityQuery WaitCooldown;
        private EntityQuery MoveToCooldown;
        private EntityQuery AttackCooldown;
        private EntityQuery RetreatCooldown;
        private EntityQuery RepairCooldown;
        private EntityQuery SpawnCooldown;
        private EntityQuery GatherCooldown;






        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            PatrolCooldown = GetEntityQuery(new EntityQueryDesc() 
            
            {All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol))},
             None = new ComponentType[] { ComponentType.ReadOnly(typeof(PatrolActionTag))}
            
            });
            TraverseCooldown = GetEntityQuery(new EntityQueryDesc()

            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Traverse)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(TraverseActionTag)) }

            });
            WaitCooldown = GetEntityQuery(new EntityQueryDesc()

            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Wait)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(WaitActionTag)) }

            });
            MoveToCooldown = GetEntityQuery(new EntityQueryDesc() {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(MoveToTarget))},
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(MoveToTargetActionTag))}
            });
            RetreatCooldown = GetEntityQuery(new EntityQueryDesc()
            {
                Any = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen))},
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(RetreatActionTag)) }
            });
            RepairCooldown = GetEntityQuery(new EntityQueryDesc()
            {
                Any = new ComponentType[] { ComponentType.ReadWrite(typeof(RepairState))},
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(HealSelfTag)) }
            });
            SpawnCooldown = GetEntityQuery(new EntityQueryDesc()
            {
                Any = new ComponentType[] { ComponentType.ReadWrite(typeof(SpawnDefendersState))},
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(SpawnTag)) }
            });
            GatherCooldown = GetEntityQuery(new EntityQueryDesc()
            {
                Any = new ComponentType[] { ComponentType.ReadWrite(typeof(GatherResourcesState)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(GatherResourcesTag)) }
            });
            AttackCooldown = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(AttackTargetState)) },
                None = new ComponentType[]{ ComponentType.ReadOnly(typeof (AttackActionTag))}
                
            });

            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new CooldownJob<Patrol>()
            {
                AIStateChunk = GetComponentTypeHandle<Patrol>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(PatrolCooldown, systemDeps);

            systemDeps = new CooldownJob<Traverse>()
            {
                AIStateChunk = GetComponentTypeHandle<Traverse>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(TraverseCooldown, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new CooldownJob<Wait>()
            {
                AIStateChunk = GetComponentTypeHandle<Wait>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(WaitCooldown, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new CooldownJob<MoveToTarget>()
            {
                AIStateChunk = GetComponentTypeHandle<MoveToTarget>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(MoveToCooldown, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new CooldownJob<RetreatCitizen>()
            {
                AIStateChunk = GetComponentTypeHandle<RetreatCitizen>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(RetreatCooldown, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new CooldownJob<AttackTargetState>()
            {
                AIStateChunk = GetComponentTypeHandle<AttackTargetState>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(AttackCooldown, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new CooldownJob<GatherResourcesState>()
            {
                AIStateChunk = GetComponentTypeHandle<GatherResourcesState>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(GatherCooldown, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new CooldownJob<RepairState>()
            {
                AIStateChunk = GetComponentTypeHandle<RepairState>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(RepairCooldown, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = new CooldownJob<SpawnDefendersState>()
            {
                AIStateChunk = GetComponentTypeHandle<SpawnDefendersState>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(SpawnCooldown, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

        [BurstCompile]
        struct CooldownJob<AISTATE> : IJobChunk
           where AISTATE : unmanaged, IBaseStateScorer
        {

            public ComponentTypeHandle<AISTATE> AIStateChunk;
            public float DT;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<AISTATE> AISTATES = chunk.GetNativeArray(AIStateChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    AISTATE aistate = AISTATES[i];
                    if (!aistate.InCooldown) {
                        continue;
                    }
                        
                    aistate.ResetTime -= DT;
                    if (aistate.ResetTime <= 0.0f)
                    { 
                        aistate.ResetTime = 0.0f;
                        aistate.Status = ActionStatus.Idle; 
                    }
                    AISTATES[i] = aistate;
                }
            }
        }
    }
}