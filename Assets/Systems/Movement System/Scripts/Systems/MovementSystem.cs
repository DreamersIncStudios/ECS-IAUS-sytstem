using UnityEngine;
using UnityEngine.AI;
using Unity.Entities;
using Components.MovementSystem;
using Unity.Transforms;
using Unity.Jobs;
using MotionSystem;
using ProjectDawn.Navigation;

namespace IAUS.ECS.Systems
{
    //[UpdateAfter(typeof(TransformSyncSystem))]
    public partial class MovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = Entities.ForEach((ref Movement movement, in LocalToWorld CurPos) =>
            {
                movement.DistanceRemaining = Vector3.Distance(movement.TargetLocation, CurPos.Position);
            }).ScheduleParallel(systemDeps);
            World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>()
                .AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;
            var agentLookup = SystemAPI.GetComponentLookup<AgentBody>();
            Entities.ForEach((ref Movement move, in Parent root) =>
            {
                var agent = agentLookup[root.Value];

                if (move.CanMove)
                {
                    //rewrite with a set position bool;
                    if (!move.SetTargetLocation) return;
                    if (!NavMesh.SamplePosition(move.TargetLocation, out var hit, 5, NavMesh.AllAreas)) return;
                    move.TargetLocation = hit.position;
                    agent.SetDestination(hit.position);

                    move.SetTargetLocation = false;
                }
                else
                {
                    agent.IsStopped = true;
                }

                agentLookup[root.Value] = agent;
            }).Run();
        }
    }
}
