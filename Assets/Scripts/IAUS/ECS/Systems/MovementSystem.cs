
using UnityEngine;
using UnityEngine.AI;
using Unity.Entities;
using IAUS.ECS.Component;
using Unity.Transforms;

namespace IAUS.ECS.System
{

    public class MovementSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((NavMeshAgent Agent,ref Movement move, ref LocalToWorld toWorld ) => {
                if (move.CanMove)
                {

                    if (Agent.destination != (Vector3)move.TargetLocation)
                    {
                        Agent.SetDestination(move.TargetLocation);
                        Agent.isStopped = false;
                        Agent.speed = move.MovementSpeed;
                    }
                    if (move.StoppingDistance>=  Vector3.Distance(toWorld.Position,Agent.destination))
                    {
                        move.CanMove = false;
                        Agent.isStopped=true;
                    }
                }
            });


        }
    }
    

}