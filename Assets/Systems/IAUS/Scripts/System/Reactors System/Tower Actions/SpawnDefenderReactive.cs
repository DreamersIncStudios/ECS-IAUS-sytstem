using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using GameModes.DestroyTheTower.TowerSystem;
using Stats;
using IAUS.NPCScriptableObj;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<SpawnTag, SpawnDefendersState, IAUS.ECS.Systems.Reactive.SpawnDefenderReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<SpawnTag, SpawnDefendersState, IAUS.ECS.Systems.Reactive.SpawnDefenderReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<SpawnTag, SpawnDefendersState, IAUS.ECS.Systems.Reactive.SpawnDefenderReactor>.ManageComponentRemovalJob))]


namespace IAUS.ECS.Systems.Reactive
{
    public struct SpawnDefenderReactor : IComponentReactorTagsForAIStates<SpawnTag, SpawnDefendersState>
    {
        public void ComponentAdded(Entity entity, ref SpawnTag newComponent, ref SpawnDefendersState AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;

        }

        public void ComponentRemoved(Entity entity, ref SpawnDefendersState AIStateCompoment, in SpawnTag oldComponent)
        {
            throw new System.NotImplementedException();
        }

        public void ComponentValueChanged(Entity entity, ref SpawnTag newComponent, ref SpawnDefendersState AIStateCompoment, in SpawnTag oldComponent)
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


        public class SpawnDefenderReactiveSystem : AIReactiveSystemBase<SpawnTag, SpawnDefendersState, SpawnDefenderReactor>
        {
            protected override SpawnDefenderReactor CreateComponentReactor()
            {
                return new SpawnDefenderReactor();
            }
        }

    }
    public partial class TowerSpawnDefenderSystem : SystemBase
    {
        private EntityQuery SpawnDefender;
        private EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            SpawnDefender = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(SpawnDefendersState)), ComponentType.ReadWrite(typeof(TowerData)),
                     ComponentType.ReadWrite(typeof(SpawnTag)), ComponentType.ReadWrite(typeof(EnemyStats))}

            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }


        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new TowerSpawnDefenderJob()
            {
                DataChunk = GetComponentTypeHandle<TowerData>(false),
                SpawnChunk = GetComponentTypeHandle<SpawnDefendersState>(false)
            }
            .ScheduleParallel(SpawnDefender, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

        struct TowerSpawnDefenderJob : IJobChunk
        {
            public ComponentTypeHandle<TowerData> DataChunk;
            public ComponentTypeHandle<SpawnDefendersState> SpawnChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<TowerData> TowerDatas = chunk.GetNativeArray(DataChunk);
                NativeArray<SpawnDefendersState> SpawnStates = chunk.GetNativeArray(SpawnChunk);
                for (int i = 0; i < chunk.Count; i++)
                {

                    TowerData data = TowerDatas[i];
                    SpawnDefendersState state = SpawnStates[i];
                    if (data.EnergyLevel > 50)
                    {
                        EnemyDatabase.GetEnemy(0).Spawn(Vector3.zero); 
                        state.DefendersActive++;
                        data.AdjustEnergy(50);
                    }
                    SpawnStates[i] = state;
                    TowerDatas[i] = data;
                }
            }
        }
    }

}