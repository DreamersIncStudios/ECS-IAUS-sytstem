
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
    [UpdateAfter(typeof(TransformSyncSystem))]
    public partial class MovementSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireForUpdate<RunningTag>();
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>().AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;

            Entities.ForEach((ref AgentBody agent, ref Movement move) =>
            {
                if (move.CanMove)
                {
                    //rewrite with a set position bool;
                    if (!move.SetTargetLocation) return;
                    if (!NavMesh.SamplePosition(move.TargetLocation, out var hit, 5, NavMesh.AllAreas)) return;
                    move.TargetLocation = hit.position;
                    agent.SetDestination(hit.position);
                    agent.IsStopped = false;
                    move.SetTargetLocation = false;

                    //if (!agent.) return;
                    //if (move.WithinRangeOfTargetLocation)
                   // {
                     //   move.CanMove = false;
                    //}
                }
                else
                {
                    agent.IsStopped = true;

                }


            }).Run();


        }


    }

}