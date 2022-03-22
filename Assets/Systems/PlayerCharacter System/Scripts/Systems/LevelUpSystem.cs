using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
namespace Stats
{
    public partial class LevelUpSystem : SystemBase
    {
        EntityCommandBufferSystem _entityCommandBufferSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            EntityCommandBuffer buffer = _entityCommandBufferSystem.CreateCommandBuffer();
            systemDeps = Entities.ForEach((Entity entity, ref PlayerStatComponent PC, ref LevelUpComponent StatUpdate) =>
            {
                PC.MaxHealth = StatUpdate.MaxHealth;
                PC.CurHealth = StatUpdate.CurHealth;
                PC.MaxMana = StatUpdate.MaxMana;
                PC.CurMana = StatUpdate.CurMana;

                buffer.RemoveComponent<LevelUpComponent>(entity);
            }).Schedule(systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            systemDeps = Entities.ForEach((Entity entity, ref EnemyStats PC, ref LevelUpComponent StatUpdate) =>
            {
                PC.MaxHealth = StatUpdate.MaxHealth;
                PC.CurHealth = StatUpdate.CurHealth;
                PC.MaxMana = StatUpdate.MaxMana;
                PC.CurMana = StatUpdate.CurMana;

                buffer.RemoveComponent<LevelUpComponent>(entity);
            }).Schedule(systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = Entities.ForEach((Entity entity, ref NPCStats PC, ref LevelUpComponent StatUpdate) =>
            {
                PC.MaxHealth = StatUpdate.MaxHealth;
                PC.CurHealth = StatUpdate.CurHealth;
                PC.MaxMana = StatUpdate.MaxMana;
                PC.CurMana = StatUpdate.CurMana;

                buffer.RemoveComponent<LevelUpComponent>(entity);
            }).Schedule(systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;
        }
    }

    public struct LevelUpComponent : IComponentData {
        public int MaxHealth;
        public int MaxMana;
        public int CurMana;
        public int CurHealth;
        public float MagicDef;
        public float MeleeAttack;
        public float MeleeDef;

    }
}