﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using IAUS.ECS2.Component;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
namespace IAUS.ECS2.Systems
{
    public class CooldownCountdownSystem : SystemBase
    {
        private EntityQuery PatrolCooldown;
        private EntityQuery WaitCooldown;
        private EntityQuery MoveToCooldown;
        private EntityQuery AttackCooldown;



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
            AttackCooldown = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(MeleeAttackTarget)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AttackTargetActionTag)) }
            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new CooldownJob<Patrol>()
            {
                AIStateChunk = GetArchetypeChunkComponentType<Patrol>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(PatrolCooldown, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new CooldownJob<Wait>()
            {
                AIStateChunk = GetArchetypeChunkComponentType<Wait>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(WaitCooldown, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new CooldownJob<MoveToTarget>()
            {
                AIStateChunk = GetArchetypeChunkComponentType<MoveToTarget>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(MoveToCooldown, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new CooldownJob<MeleeAttackTarget>()
            {
                AIStateChunk = GetArchetypeChunkComponentType<MeleeAttackTarget>(false),
                DT = Time.DeltaTime
            }.ScheduleParallel(AttackCooldown, systemDeps);

            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;
        }

        [BurstCompile]
        struct CooldownJob<AISTATE> : IJobChunk
           where AISTATE : unmanaged, IBaseStateScorer
        {

            public ArchetypeChunkComponentType<AISTATE> AIStateChunk;
            public float DT;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<AISTATE> AISTATES = chunk.GetNativeArray(AIStateChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    AISTATE aistate = AISTATES[i];
                    if (!aistate.InCooldown) {
                        return;
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