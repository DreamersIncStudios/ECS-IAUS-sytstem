
using UnityEngine;
using UnityEngine.AI;
using Unity.Entities;
using Components.MovementSystem;
using Unity.Transforms;
using Unity.Jobs;
using MotionSystem;
using UnityEditor;

namespace IAUS.ECS.Systems
{
    [UpdateAfter(typeof(TransformSyncSystem))]
    public partial class MovementSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = Entities.ForEach((ref Movement movement, in LocalTransform CurPos) =>
            {
                movement.DistanceRemaining = Vector3.Distance(movement.TargetLocation, CurPos.Position);
            }).ScheduleParallel(systemDeps);
            World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>().AddJobHandleForProducer(systemDeps);
            Dependency = systemDeps;

            Entities.WithoutBurst().ForEach((NavMeshAgent Agent, ref Movement move) =>
            {
                if (move.CanMove)
                {
                    //rewrite with a set position bool;
                    if (move.SetTargetLocation)
                    {
                        if (NavMesh.SamplePosition(move.TargetLocation, out var hit, 10, NavMesh.AllAreas))
                            move.TargetLocation = hit.position;
                        Debug.Log(hit.position);
                        Agent.SetDestination(move.TargetLocation);
                        Agent.isStopped = false;
                        move.SetTargetLocation = false;
                    }



                    if (Agent.hasPath)
                    {
                        if (move.WithinRangeOfTargetLocation)
                        {
                            move.CanMove = false;
                        }
                    }
                }
                else
                {
                    Agent.isStopped = true;

                }


            }).Run();


        }


    }

}