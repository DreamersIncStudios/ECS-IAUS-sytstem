
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
                    if (Agent.hasPath)
                    {
                        float dist = Vector3.Distance(Agent.destination, Agent.transform.position);
                        if ( dist<=.7f)
                        {
                            Debug.Log(true);
                            move.CanMove = false;
                            Agent.isStopped = true;
                            move.Completed = true;
                        }
                    }
                }
                else {
                    Agent.isStopped = false;

                }
            });


        }
    }
    

}