using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Entities;
using MotionSystem.Tower;
using Unity.Transforms;

namespace IAUS.ECS.Systems.Reactive
{
    public partial class StaticUnitsAttackSystem : SystemBase
    {
        EntityQuery TagAdded;
        EntityQuery TagRemoved;
        EntityQuery TagValueChange;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            TagAdded = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(TowerController)), ComponentType.ReadOnly(typeof(AttackActionTag)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>.StateComponent)) }

            });
            TagRemoved = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(TowerController)), ComponentType.ReadOnly(typeof(AttackActionTag)), ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>.StateComponent)) },

            });
            TagValueChange = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(TowerController)), ComponentType.ReadOnly(typeof(AIReactiveSystemBase<AttackActionTag, AttackTargetState, AttackTagReactor>.StateComponent)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AttackActionTag)) }

            });
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new SetRotationTagJob {
                AttackInfoChunk = GetComponentTypeHandle<AttackActionTag>(true),
                EntityChunk = GetEntityTypeHandle(),
                TowerChunk = GetComponentTypeHandle<TowerController>(false),
                TransformChunk = GetComponentTypeHandle<LocalToWorld>(true),
                ECB =_entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter()
            }.ScheduleParallel(TagAdded, systemDeps);
            systemDeps.Complete();
            Dependency = systemDeps;
        }

        public struct SetRotationTagJob : IJobChunk
        {
            [ReadOnly] public EntityTypeHandle EntityChunk;
            [ReadOnly] public ComponentTypeHandle<AttackActionTag> AttackInfoChunk;
            public ComponentTypeHandle<TowerController> TowerChunk;
          [ReadOnly]  public ComponentTypeHandle<LocalToWorld> TransformChunk;

            public EntityCommandBuffer.ParallelWriter ECB;
            
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                NativeArray<AttackActionTag> actions = chunk.GetNativeArray(AttackInfoChunk);
                NativeArray <TowerController> towers = chunk.GetNativeArray(TowerChunk);
                NativeArray<LocalToWorld> local = chunk.GetNativeArray(TransformChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    TowerController tower = towers[i];
                    ECB.AddComponent(chunkIndex,entities[i], new RotateTowerTag {
                        TargetPosition = actions[i].AttackLocation
                    });
                    tower.forward = local[i].Forward;
                    towers[i] = tower;
                }
            }
        }
    }

}