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
    public class CooldownCountdownSystem : SystemBase
    {
        private EntityQuery PatrolCooldown;
        private EntityQuery WaitCooldown;
        private EntityQuery MoveToCooldown;
        private EntityQuery AttackCooldown;
        private EntityQuery RetreatCooldown;




        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            PatrolCooldown = GetEntityQuery(new EntityQueryDesc() 
            
            {All = new ComponentType[] { ComponentType.ReadWrite(typeof(Patrol))},
             None = new ComponentType[] { ComponentType.ReadOnly(typeof(PatrolActionTag))}
            
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
                Any = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)),/* ComponentType.ReadWrite(typeof(RetreatPlayerPartyNPC)) , ComponentType.ReadWrite(typeof(RetreatEnemyNPC)) */},
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(RetreatActionTag)) }
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