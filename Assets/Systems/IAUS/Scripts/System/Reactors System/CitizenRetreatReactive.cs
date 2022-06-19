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
            _componentRemovedQuery = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(RetreatCitizen)),  ComponentType.ReadWrite(typeof(Movement))
                , ComponentType.ReadOnly(typeof(AIReactiveSystemBase<RetreatActionTag, RetreatCitizen, RetreatCitizenTagReactor>.StateComponent)) },
                None = new ComponentType[] { ComponentType.ReadOnly(typeof(RetreatActionTag)) }

            });


            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }

        protected override void OnUpdate()
        {

            JobHandle systemDeps = Dependency;
            Entities.WithoutBurst().ForEach((ref Movement move, ref Wait wait, ref RetreatCitizen retreat, ref Patrol patrol) =>
            {

                if (retreat.Status == ActionStatus.Running)
                {
                    wait.Timer = 0.0f;
                    start:
                    if (NavMesh.SamplePosition(retreat.LocationOfLowestThreat, out NavMeshHit hit, 6.0f, NavMesh.AllAreas))
                    {
                        move.SetLocation ( hit.position);
                        Debug.LogError("Bug is Here");
                        patrol.StartingDistance = patrol.distanceToPoint = 1.0f;
                    }
                    else
                    {
                        goto start;
                    }
                }
            }).Run();

            systemDeps = new OnCompletionUpdateJob()
            {
                MovementChunk = GetComponentTypeHandle<Movement>(false),
                WaitChunk = GetComponentTypeHandle<Wait>(false),
                Buffer = GetBufferFromEntity<TravelWaypointBuffer>(false),
                CanPatrol = GetComponentDataFromEntity<Patrol>(false),
                EntityChunk = GetEntityTypeHandle(),
                RetreatChunk = GetComponentTypeHandle<RetreatCitizen>(false),
                ToWorldChunk = GetComponentTypeHandle<LocalToWorld>(true)

            }.ScheduleParallel(_componentRemovedQuery, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;

        }
        
        public struct RetreatAdded : IJobChunk
        {
            public ComponentTypeHandle<Wait> WaitChunk;
            public ComponentTypeHandle<Movement> MovementChunk;
            public ComponentTypeHandle<RetreatCitizen> RetreatChunk;
            public ComponentTypeHandle<Patrol> PatrolChunk;

            [BurstDiscard]
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
                NativeArray<Movement> Moves = chunk.GetNativeArray(MovementChunk);
                NativeArray<RetreatCitizen> retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<Patrol> Patrols = chunk.GetNativeArray(PatrolChunk);
                for (int i = 0; i<chunk.Count; i++)
			{
                    Wait wait = Waits[i];
                    Movement move = Moves[i];
                    Patrol patrol = Patrols[i];
                    wait.Timer = 0.0f;
                    start:
                    if (NavMesh.SamplePosition(retreats[i].LocationOfLowestThreat, out NavMeshHit hit, 6.0f, NavMesh.AllAreas))
                    {
                        Debug.Log("Move to retreat Point");

                        move.SetLocation(hit.position);
                        patrol.StartingDistance = patrol.distanceToPoint = 1.0f;
                    }
                    else
                    {
                        goto start;
                    }

                    Waits[i] = wait;
                    Moves[i] = move;
                    Patrols[i] = patrol;
                }
        }

        }
        [BurstCompile]
        public struct OnCompletionUpdateJob : IJobChunk
        {
            public ComponentTypeHandle<Wait> WaitChunk;
            public ComponentTypeHandle<Movement> MovementChunk;
            public ComponentTypeHandle<RetreatCitizen> RetreatChunk;
            [ReadOnly] public ComponentTypeHandle<LocalToWorld> ToWorldChunk;

           [NativeDisableParallelForRestriction] public ComponentDataFromEntity<Patrol> CanPatrol;
            [ReadOnly]public BufferFromEntity<TravelWaypointBuffer> Buffer;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
                NativeArray<Movement> Moves = chunk.GetNativeArray(MovementChunk);
                NativeArray<RetreatCitizen> retreats = chunk.GetNativeArray(RetreatChunk);
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                NativeArray<LocalToWorld> ToWorlds = chunk.GetNativeArray(ToWorldChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Wait wait = Waits[i];
                    Movement move = Moves[i];
                    //TODO create a hide state 
                     //   wait.Timer = retreats[i].HideTime;
                    if (CanPatrol.HasComponent(entities[i])) {
                        Patrol patrol = CanPatrol[entities[i]];
                        //TODO needs to be rewritten to account for influence by group.
                        if (patrol.CurWaypoint.InfluenceAtPosition.y > .7 && patrol.CurWaypoint.InfluenceAtPosition.y < .75f) 
                        {
                            //move to the next point
                            patrol.WaypointIndex++;
                            if (patrol.WaypointIndex >= patrol.NumberOfWayPoints)
                                patrol.WaypointIndex = 0;

                            patrol.CurWaypoint = Buffer[entities[i]][patrol.WaypointIndex].WayPoint;

                            patrol.StartingDistance = Vector3.Distance(ToWorlds[i].Position, patrol.CurWaypoint.Position);

                            CanPatrol[entities[i]] = patrol;
                        }
                    }
                    move.CanMove = false;

                    Waits[i] = wait;
                    Moves[i] = move;
                }
            }
        }

    }
    
}