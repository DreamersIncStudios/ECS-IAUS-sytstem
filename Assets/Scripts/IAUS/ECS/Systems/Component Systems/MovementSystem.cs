
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

                    if (!move.TargetLocation.Equals(Agent.destination))
                    {
                        Agent.SetDestination(move.TargetLocation);
                        Agent.isStopped = false;
                      //  Agent.speed = move.MovementSpeed;
                    }
                    if (move.StoppingDistance >= Agent.remainingDistance)
                    {
                        move.CanMove = false;
                        Agent.isStopped = true;
                        move.Completed = true;
                    }
                }
                else {
                    Agent.isStopped = false;

                }
            });


        }
    }
    

}