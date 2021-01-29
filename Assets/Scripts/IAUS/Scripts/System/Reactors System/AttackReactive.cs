using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using IAUS.ECS2.Component;
using Unity.Entities;
using Unity.Burst;
using Stats;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<AttackTargetActionTag,MeleeAttackTarget, IAUS.ECS2.Systems.Reactive.MeleeAttackReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AttackTargetActionTag, MeleeAttackTarget, IAUS.ECS2.Systems.Reactive.MeleeAttackReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<AttackTargetActionTag, MeleeAttackTarget, IAUS.ECS2.Systems.Reactive.MeleeAttackReactor>.ManageComponentRemovalJob))]



namespace IAUS.ECS2.Systems.Reactive
{
    public struct MeleeAttackReactor : IComponentReactorTagsForAIStates<AttackTargetActionTag, MeleeAttackTarget>
    {
        public void ComponentAdded(Entity entity, ref AttackTargetActionTag newComponent, ref MeleeAttackTarget AIStateCompoment)
        {
        }

        public void ComponentRemoved(Entity entity, ref MeleeAttackTarget AIStateCompoment, in AttackTargetActionTag oldComponent)
        {

                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
        }

        public void ComponentValueChanged(Entity entity, ref AttackTargetActionTag newComponent, ref MeleeAttackTarget AIStateCompoment, in AttackTargetActionTag oldComponent)
        {
        
        }

        public class MeleeAttackReactiveSystem : AIReactiveSystemBase<AttackTargetActionTag, MeleeAttackTarget, MeleeAttackReactor>
        {
            protected override MeleeAttackReactor CreateComponentReactor()
            {
                return new MeleeAttackReactor();
            }
        }
    }

    public class AttackSystem : SystemBase
    {
        EntityQuery MeleeAttackers;
        EntityCommandBufferSystem entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            MeleeAttackers = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(MeleeAttackTarget)), ComponentType.ReadOnly(typeof(AttackTargetActionTag)), ComponentType.ReadOnly(typeof(CharacterStatComponent)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackTargetActionTag,MeleeAttackTarget, MeleeAttackReactor>.StateComponent))}

            });
        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new MeleeJob() {
                StatsChunk = GetComponentTypeHandle< CharacterStatComponent >(true),
                MeleeChunk = GetComponentTypeHandle<MeleeAttackTarget>(false)
            }.ScheduleParallel(MeleeAttackers, systemDeps);
            entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;

        }

        //[BurstCompile]
        struct MeleeJob : IJobChunk
        {
            public ComponentTypeHandle<MeleeAttackTarget> MeleeChunk;
            [ReadOnly]public ComponentTypeHandle<CharacterStatComponent> StatsChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<MeleeAttackTarget> Melees = chunk.GetNativeArray(MeleeChunk);
                NativeArray<CharacterStatComponent> Stats = chunk.GetNativeArray(StatsChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    MeleeAttackTarget melee = Melees[i];
                    melee.Timer = 15; // need to create attack interval 

                    Debug.Log("Attacked Target out damage = " + Stats[i].MeleeAttack.ToString());
                    Melees[i] = melee;
                }

            }
        }

    }

}