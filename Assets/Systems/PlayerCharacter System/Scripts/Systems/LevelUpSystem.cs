using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
namespace Stats
{
    public class LevelUpSystem : SystemBase
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
                PC.MaxMana = StatUpdate.MaxMana;
                PC.MeleeAttack = StatUpdate.MeleeAttack;
                PC.MeleeDef = StatUpdate.MeleeDef;
                PC.MagicDef = StatUpdate.MagicDef;

                buffer.RemoveComponent<LevelUpComponent>(entity);
            }).Schedule(systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;
        }
    }

    public struct LevelUpComponent : IComponentData {
        public int MaxHealth;
        public int MaxMana;

        public float MagicDef;
        public float MeleeAttack;
        public float MeleeDef;

    }
}