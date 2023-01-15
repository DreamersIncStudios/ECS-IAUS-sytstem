using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using IAUS.ECS.Component;
using Unity.Burst;
using Unity.Transforms;
using UnityEngine;
using AISenses;
using Utilities.ReactiveSystem;
using IAUS.ECS.Systems.Reactive;
using Components.MovementSystem;
using Unity.Physics;


namespace IAUS.ECS.Systems {

    [UpdateAfter(typeof(IAUSUpdateGroup))]
    public class IAUSUpdateStateGroup : ComponentSystemGroup
    {
        public IAUSUpdateStateGroup()
        {
            RateManager = new RateUtils.VariableRateManager(6000, true);

        }

    }
    [UpdateInGroup(typeof(IAUSUpdateStateGroup))]
    public partial class UpdateTerrorizeAction : SystemBase
    {

        private EntityQuery _componentAddedQuery;
        private EntityQuery _terrorize;
        EntityCommandBufferSystem _entityCommandBufferSystem;


        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(TerrorizeAreaState)), ComponentType.ReadWrite(typeof(TerrorizeAreaTag)),
                    ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer)), ComponentType.ReadOnly(typeof(LocalToWorld))
                },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<TerrorizeAreaTag, TerrorizeAreaState, TerrorizeReactor>.StateComponent)) }
            });
            _terrorize = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(TerrorizeAreaState)), ComponentType.ReadWrite(typeof(TerrorizeAreaTag)), ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(TravelWaypointBuffer))
                , ComponentType.ReadOnly(typeof(LocalToWorld)), ComponentType.ReadOnly(typeof(AIReactiveSystemBase<TerrorizeAreaTag, TerrorizeAreaState, TerrorizeReactor>.StateComponent)) }
            });

        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new FindTargetToTerrorize() { 
                EnemyChunk = GetBufferTypeHandle<ScanPositionBuffer>(true),
                TerrorChunk = GetComponentTypeHandle<TerrorizeAreaState>(false)
            }.Schedule(_componentAddedQuery,systemDeps);
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            systemDeps = new FindTargetToTerrorize() {
                EnemyChunk = GetBufferTypeHandle<ScanPositionBuffer>(true),
                TerrorChunk = GetComponentTypeHandle<TerrorizeAreaState>(false)
            }.Schedule(_terrorize, systemDeps);
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            systemDeps = new LookForTargetAndUpdate() {
                Positions = GetComponentDataFromEntity<LocalToWorld>(true),
                TerrorChunk = GetComponentTypeHandle<TerrorizeAreaState>(false),
                EntityChunk = GetEntityTypeHandle()
            }.Schedule(_terrorize, systemDeps);
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            systemDeps = new MoveToTargetToTerrrize() { 
                MoveChunk = GetComponentTypeHandle<Movement>(false),
                TerrorChunk = GetComponentTypeHandle<TerrorizeAreaState>(false)
            }.Schedule(_terrorize,systemDeps);
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
           
            systemDeps = new Attack()
            {
                TerrorChunk = GetComponentTypeHandle<TerrorizeAreaState>(false)
            }.Schedule(_terrorize, systemDeps);
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

            Dependency = systemDeps;

        }

        [BurstCompile]
        public struct LookForTargetAndUpdate : IJobChunk
        {
            [NativeDisableParallelForRestriction]
            [ReadOnly] public ComponentDataFromEntity<LocalToWorld> Positions;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public ComponentTypeHandle<TerrorizeAreaState> TerrorChunk;


            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                NativeArray<TerrorizeAreaState> terrors = chunk.GetNativeArray(TerrorChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    TerrorizeAreaState terror = terrors[i];
                    if (terror.HasAttack)
                    {
                        float dist = Vector3.Distance(Positions[entities[i]].Position, Positions[terror.attackThis.entity].Position);
                        if (dist < terror.MaxTerrorizeRadius)
                        {
                            terror.attackThis.DistanceTo = dist;
                            terror.attackThis.LastKnownPosition = Positions[terror.attackThis.entity].Position;
                        }
                        else
                        {
                            terror.terrorizeSubstate = TerrorizeSubstates.FindTarget;
                        }
                    }
                    terrors[i] = terror;

                }
            }
        }
        [BurstCompile]
        public struct MoveToTargetToTerrrize : IJobChunk
        {
            public ComponentTypeHandle<TerrorizeAreaState> TerrorChunk;
            public ComponentTypeHandle<Movement> MoveChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<TerrorizeAreaState> terrors = chunk.GetNativeArray(TerrorChunk);
                NativeArray<Movement> moves = chunk.GetNativeArray(MoveChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    TerrorizeAreaState terror = terrors[i];
                    Movement move = moves[i];
                    if (terror.terrorizeSubstate != TerrorizeSubstates.MoveToTarget)
                        continue;

                    move.SetLocation(terror.attackThis.LastKnownPosition);
                    if (move.WithinRangeOfTargetLocation) {
                        terror.terrorizeSubstate = TerrorizeSubstates.AttackTarget;
                    }

                    moves[i]= move;
                    terrors[i]= terror;
                }
            }
        }

        [BurstCompile]
        public struct FindTargetToTerrorize : IJobChunk
        {
            [ReadOnly] public BufferTypeHandle<ScanPositionBuffer> EnemyChunk;
            public ComponentTypeHandle<TerrorizeAreaState> TerrorChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<TerrorizeAreaState> terrors = chunk.GetNativeArray(TerrorChunk);
                BufferAccessor<ScanPositionBuffer> bufferAccessor = chunk.GetBufferAccessor(EnemyChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    TerrorizeAreaState terror = terrors[i];
                    DynamicBuffer<ScanPositionBuffer> buffer = bufferAccessor[i];
                    if (buffer.IsEmpty || terror.terrorizeSubstate != TerrorizeSubstates.FindTarget)
                    {
                        continue;
                    }
                    else
                    {
                        if (buffer.Length > 1)
                        {
                            terror.attackThis = buffer[0].target;
                            for (int j = 1; j < buffer.Length; j++)
                            {
                                if (terror.attackThis.DistanceTo > buffer[j].target.DistanceTo && buffer[j].target.CanSee)
                                    terror.attackThis = buffer[j];
                            }
                        }
                        terror.terrorizeSubstate = TerrorizeSubstates.MoveToTarget;
                    }
                    terrors[i] = terror;

                }
            }
        }

        [BurstCompile]
        public struct Attack : IJobChunk
        {
            public ComponentTypeHandle<TerrorizeAreaState> TerrorChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {

                NativeArray<TerrorizeAreaState> terrors = chunk.GetNativeArray(TerrorChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    TerrorizeAreaState terror = terrors[i];
                    if (terror.terrorizeSubstate != TerrorizeSubstates.AttackTarget)
                        continue;
                    if (terror.attackThis.DistanceTo < 2.5)
                    {
                        Debug.Log("Attack");
                    }
                    else {
                        terror.terrorizeSubstate = TerrorizeSubstates.MoveToTarget;
                    }
                    terrors[i] = terror;


                }
            }
        }


    }


}
