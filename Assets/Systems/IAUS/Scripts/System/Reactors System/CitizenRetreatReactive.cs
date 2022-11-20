using Unity.Collections;
using UnityEngine;
using Utilities.ReactiveSystem;
using Unity.Jobs;
using Unity.Transforms;
using IAUS.ECS.Component;
using Unity.Entities;
using Unity.Burst;
using UnityEngine.AI;
using Components.MovementSystem;

[assembly: RegisterGenericComponentType(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, IAUS.ECS.Systems.Reactive.RetreatCitizenTagReactor>.StateComponent))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, IAUS.ECS.Systems.Reactive.RetreatCitizenTagReactor>.ManageComponentAdditionJob))]
[assembly: RegisterGenericJobType(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, IAUS.ECS.Systems.Reactive.RetreatCitizenTagReactor>.ManageComponentRemovalJob))]


namespace IAUS.ECS.Systems.Reactive
{
    public struct RetreatCitizenTagReactor : IComponentReactorTagsForAIStates<RetreatActionTag, RetreatCitizen>
    {
        public void ComponentAdded(Entity entity, ref RetreatActionTag newComponent, ref RetreatCitizen AIStateCompoment)
        {
            AIStateCompoment.Status = ActionStatus.Running;
            Debug.Log("Escape");
        }

        public void ComponentRemoved(Entity entity, ref RetreatCitizen AIStateCompoment, in RetreatActionTag oldComponent)
        {

            AIStateCompoment.Status = ActionStatus.CoolDown;
            AIStateCompoment.ResetTime = AIStateCompoment.CoolDownTime;
        }

        public void ComponentValueChanged(Entity entity, ref RetreatActionTag newComponent, ref RetreatCitizen AIStateCompoment, in RetreatActionTag oldComponent)
        {
        }

        public class RetreatReactiveSystem : AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>
        {
            protected override RetreatCitizenTagReactor CreateComponentReactor()
            {
                return new RetreatCitizenTagReactor();
            }
        }
    }

    [UpdateInGroup(typeof(IAUSUpdateGroup))]

    public sealed partial class RetreatMovement : SystemBase
    {
        private EntityQuery _componentAddedQuery;
        private EntityQuery _componentAddedQueryWithWait;

        private EntityQuery _componentRemovedQuery;
        EntityCommandBufferSystem _entityCommandBufferSystem;


        protected override void OnCreate()
        {
            base.OnCreate();

            _componentAddedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)),  ComponentType.ReadWrite(typeof(Movement)),ComponentType.ReadWrite(typeof(Traverse))
                , ComponentType.ReadOnly(typeof(RetreatActionTag))},
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>.StateComponent)) }

            });

            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)),  ComponentType.ReadWrite(typeof(Movement)),ComponentType.ReadWrite(typeof(Traverse))
                , ComponentType.ReadOnly(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>.StateComponent)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(RetreatActionTag)) }

            });


            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {

            JobHandle systemDeps = Dependency;

            systemDeps = new RetreatAdded()
            {
                WaitChunk = GetComponentTypeHandle<Wait>(false),
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(false),
                TravelBufferChunk = GetBufferTypeHandle<TravelWaypointBuffer>(false),
                TraverseChunk = GetComponentTypeHandle<Traverse>(false),
                toWorldChunk = GetComponentTypeHandle<LocalToWorld>(true)
            }.Schedule(_componentAddedQuery, systemDeps);

            Dependency = systemDeps;

        }


        public struct RetreatAdded : IJobChunk
        {
            public ComponentTypeHandle<Wait> WaitChunk;
            public ComponentTypeHandle<Movement> MovementChunk;
            public ComponentTypeHandle<RetreatCitizen> RetreatChunk;
            public ComponentTypeHandle<Traverse> TraverseChunk;
            [ReadOnly]public ComponentTypeHandle<LocalToWorld> toWorldChunk;
            public BufferTypeHandle<TravelWaypointBuffer> TravelBufferChunk;

            [BurstDiscard]
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
                NativeArray<Movement> Moves = chunk.GetNativeArray(MovementChunk);
                NativeArray<RetreatCitizen> retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<Traverse> Traverses = chunk.GetNativeArray(TraverseChunk);
                NativeArray<LocalToWorld> toWorlds = chunk.GetNativeArray(toWorldChunk);
                BufferAccessor<TravelWaypointBuffer> bufferAccessor = chunk.GetBufferAccessor(TravelBufferChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Wait wait = Waits[i];
                    Movement move = Moves[i];
                    Traverse traverse = Traverses[i];
                    DynamicBuffer<TravelWaypointBuffer> buffer = bufferAccessor[i];
                    wait.Timer = 0.0f;
                    start:
                    if (NavMesh.SamplePosition(retreats[i].LocationOfLowestThreat, out NavMeshHit hit, 6.0f, NavMesh.AllAreas))
                    {
                        Debug.Log("Move to retreat Point");
                        move.SetLocation(hit.position);
                        traverse.StartingDistance = traverse.distanceToPoint = 1.0f;
                        if (traverse.CurWaypoint.InfluenceAtPosition.y > .7 && traverse.CurWaypoint.InfluenceAtPosition.y < .75f)
                        {
                            var temp = buffer[traverse.WaypointIndex];
                            temp.WayPoint.Avoid = true;
                            buffer[traverse.WaypointIndex] = temp;

                            traverse.WaypointIndex++;
                            if (traverse.WaypointIndex >= traverse.NumberOfWayPoints)
                                traverse.WaypointIndex = 0;

                            traverse.CurWaypoint = buffer[traverse.WaypointIndex].WayPoint;

                            traverse.StartingDistance = Vector3.Distance(toWorlds[i].Position, traverse.CurWaypoint.Position);
                        }
                    }
                    else
                    {
                        goto start;
                    }

                    Waits[i] = wait;
                    Moves[i] = move;
                    Traverses[i] = traverse;
                }
            }


        }
    }
}


