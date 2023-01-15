using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using GameModes.DestroyTheTower.TowerSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<GatherResourcesTag, GatherResourcesState, IAUS.ECS.Systems.Reactive.GatherResourcesTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<GatherResourcesTag, GatherResourcesState, IAUS.ECS.Systems.Reactive.GatherResourcesTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<GatherResourcesTag, GatherResourcesState, IAUS.ECS.Systems.Reactive.GatherResourcesTagReactor>.ManageComponentRemovalJob))]


namespace IAUS.ECS.Systems.Reactive
{

    public struct GatherResourcesTagReactor : IComponentReactorTagsForAIStates<GatherResourcesTag, GatherResourcesState>
    {
        public void ComponentAdded(Entity entity, ref GatherResourcesTag newComponent, ref GatherResourcesState AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
        }

        public void ComponentRemoved(Entity entity, ref GatherResourcesState AIStateCompoment, in GatherResourcesTag oldComponent)
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

        public void ComponentValueChanged(Entity entity, ref GatherResourcesTag newComponent, ref GatherResourcesState AIStateCompoment, in GatherResourcesTag oldComponent)
        {
        }
        public class GatherResourceReactiveSystem : AIReactiveSystemBase<GatherResourcesTag, GatherResourcesState, GatherResourcesTagReactor>
        {


            protected override GatherResourcesTagReactor CreateComponentReactor()
            {
                return new GatherResourcesTagReactor();
            }
        }
    }
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class GatherResourcesSystem : SystemBase
    {
        private EntityQuery Gathering;
        private EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            Gathering = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(GatherResourcesState)), ComponentType.ReadWrite(typeof(TowerData)),
                     ComponentType.ReadWrite(typeof(GatherResourcesTag))}

            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new GatherResourcesJob()
            {
                DataChunk = GetComponentTypeHandle<TowerData>(false)
            }
            .ScheduleParallel(Gathering, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

        [BurstCompile]
        struct GatherResourcesJob : IJobChunk
        {
            public ComponentTypeHandle<TowerData> DataChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<TowerData> TowerDatas = chunk.GetNativeArray(DataChunk);
                for (int i = 0; i < chunk.Count; i++)
                {

                    TowerData data = TowerDatas[i];
                    data.ResourcesGathered += data.GatherResourcesRateFixed;

                    TowerDatas[i] = data;
                }

            }
        }
    }
}

