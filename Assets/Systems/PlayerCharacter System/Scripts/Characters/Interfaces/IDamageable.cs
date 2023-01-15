using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Stats;
using Stats.UI;
using Unity.Entities;
namespace DreamersInc.DamageSystem.Interfaces
{
    public interface IDamageable
    {
        bool Dead { get; }
        Entity SelfEntityRef { get; }
        void TakeDamage(int Amount, TypeOfDamage typeOf, Element element);
        void ReactToHit(float impact, Vector3 Test, Vector3 forward , TypeOfDamage typeOf = TypeOfDamage.Melee , Element element = Element.None);
    }


    public enum TypeOfDamage {Melee, MagicAoE, Projectile, Recovery, Magic}
    public enum Element { None, Fire, Water, Earth, Wind, Ice, Holy, Dark}
    public struct AdjustHealth : IComponentData {
        public int Value;
    }
    public struct AdjustMana : IComponentData
    {
        public int Value;   
    }

    public partial class AdjustVitalsSystem : SystemBase
    {
        private EntityQuery enemyQuery;
        private EntityQuery playerQuery;

        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            
            enemyQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(EnemyStats)),ComponentType.ReadOnly(typeof(AdjustHealth))}
            });
            playerQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(PlayerStatComponent)), ComponentType.ReadOnly(typeof(AdjustHealth)) }
            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new AdjustHealthJob<EnemyStats>() {
                HealthChunk = GetComponentTypeHandle<EnemyStats>(false),
                ModChunk = GetComponentTypeHandle<AdjustHealth>(true),
                EntityChunk = GetEntityTypeHandle(),
                player = false,
                ECBP = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter()
            }.ScheduleParallel(enemyQuery, systemDeps);
            systemDeps.Complete();
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new AdjustHealthJob<PlayerStatComponent>()
            {
                HealthChunk = GetComponentTypeHandle<PlayerStatComponent>(false),
                ModChunk = GetComponentTypeHandle<AdjustHealth>(true),
                EntityChunk = GetEntityTypeHandle(),
                player = true,
                ECBP = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter()
            }.ScheduleParallel(playerQuery, systemDeps);
            systemDeps.Complete();

            Dependency = systemDeps;
        }

        public struct AdjustHealthJob<STAT> : IJobChunk
            where STAT : unmanaged, StatsComponent
        {
            public ComponentTypeHandle<STAT> HealthChunk;
           [ReadOnly] public ComponentTypeHandle<AdjustHealth> ModChunk;
            public EntityCommandBuffer.ParallelWriter ECBP;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public bool player;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<STAT> Healths = chunk.GetNativeArray(HealthChunk);
                NativeArray<AdjustHealth> mods = chunk.GetNativeArray(ModChunk);
                NativeArray<Entity> entity = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    STAT Health = Healths[i];
                    Health.AdjustHealth(mods[i].Value);
                    if (Health.CurHealth <= 0) {
                        ECBP.AddComponent<EntityHasDiedTag>(chunkIndex, entity[i]);
                    }
                    if (player)
                    {
                        Object.FindObjectOfType<StatsUI>().UpdateHealthBar(Health.CurHealth);
                    }

                    ECBP.RemoveComponent<AdjustHealth>(chunkIndex, entity[i]);
                    Healths[i] = Health;

                }
            }
        }
        public struct AdjustManaJob<STAT> : IJobChunk
               where STAT : unmanaged, StatsComponent
        {
            public ComponentTypeHandle<STAT> ManaChunk;
            public ComponentTypeHandle<AdjustMana> ModChunk;
            public EntityCommandBuffer.ParallelWriter ECBP;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public ComponentDataFromEntity<PlayerStatComponent> player;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<STAT> Manas = chunk.GetNativeArray(ManaChunk);
                NativeArray<AdjustMana> mods = chunk.GetNativeArray(ModChunk);
                NativeArray<Entity> entity = chunk.GetNativeArray(EntityChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    STAT mana = Manas[i];
                    mana.AdjustHealth(mods[i].Value);
                    if (player.HasComponent(entity[i]))
                    {
                        Object.FindObjectOfType<StatsUI>().UpdateManaBar(mana.CurMana);
                    }
                    Manas[i] = mana;
                    ECBP.RemoveComponent<AdjustMana>(chunkIndex, entity[i]);
                }
            }
        }


    }
}