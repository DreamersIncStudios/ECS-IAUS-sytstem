using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using GameModes.DestroyTheTower.TowerSystem;
using Stats;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<HealSelfTag, RepairState, IAUS.ECS.Systems.Reactive.RepairTowerTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<HealSelfTag, RepairState, IAUS.ECS.Systems.Reactive.RepairTowerTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<HealSelfTag, RepairState, IAUS.ECS.Systems.Reactive.RepairTowerTagReactor>.ManageComponentRemovalJob))]

namespace IAUS.ECS.Systems.Reactive
{
    public struct RepairTowerTagReactor : IComponentReactorTagsForAIStates<HealSelfTag, RepairState>
    {
        public void ComponentAdded(Entity entity, ref HealSelfTag newComponent, ref RepairState AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref RepairState AIStateCompoment, in HealSelfTag oldComponent)
        {
            if (AIStateCompoment.Complete)
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
            }
            else
            {
                AIStateCompoment.Status = ActionStatus.CoolDown;
                AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime * 2;

            }
        }

        public void ComponentValueChanged(Entity entity, ref HealSelfTag newComponent, ref RepairState AIStateCompoment, in HealSelfTag oldComponent)
        {
        }

        public class RepairTowerReactiveSystem : AIReactiveSystemBase<HealSelfTag, RepairState, RepairTowerTagReactor>
        {
            protected override RepairTowerTagReactor CreateComponentReactor()
            {
                return new RepairTowerTagReactor();
            }
        }
    }


    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class RepairSystem : SystemBase
    {
        private EntityQuery Repairing;
        private EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            Repairing = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(RepairState)), ComponentType.ReadWrite(typeof(TowerData)),
                     ComponentType.ReadWrite(typeof(HealSelfTag)), ComponentType.ReadWrite(typeof(EnemyStats))}

            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new RepairTowerJob()
            {
                DataChunk = GetComponentTypeHandle<TowerData>(true) ,
                StatsChunk = GetComponentTypeHandle<EnemyStats>(false)
            }
            .ScheduleParallel(Repairing, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

        [BurstCompile]
        struct RepairTowerJob : IJobChunk
        {
            public ComponentTypeHandle<TowerData> DataChunk;
            public ComponentTypeHandle<EnemyStats> StatsChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<TowerData> TowerDatas = chunk.GetNativeArray(DataChunk);
                NativeArray<EnemyStats> Stats = chunk.GetNativeArray(StatsChunk);
                for (int i = 0; i < chunk.Count; i++)
                {

                    TowerData data = TowerDatas[i];
                    EnemyStats stat = Stats[i];
                    if (data.EnergyLevel > 0)
                    {
                        stat.AdjustHealth(Mathf.RoundToInt(data.RepairRateFixed));
                        data.AdjustEnergy(-data.RepairRateFixed * 2);
                    }
                    TowerDatas[i] = data;
                    Stats[i] = stat;
                }

            }
        }
    }

}