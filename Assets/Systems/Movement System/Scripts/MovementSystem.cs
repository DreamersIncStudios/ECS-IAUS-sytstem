
using UnityEngine;
using UnityEngine.AI;
using Unity.Entities;
using Components.MovementSystem;
using Unity.Transforms;
using IAUS.Core;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace IAUS.ECS.System
{
   //[UpdateAfter(typeof(IAUS_UpdateState))]
    public class MovementSystem : ComponentSystem
    {

        protected override void OnUpdate()
        {


            Entities.ForEach((NavMeshAgent Agent, ref Movement move) =>
            {
                if (move.CanMove)
                {
                    //rewrite with a set position bool;
                    if (move.SetTargetLocation)
                    {
                        //  Agent.ResetPath();
                        Agent.SetDestination(move.TargetLocation);
                        Agent.isStopped = false;
                        move.SetTargetLocation = false;
                        //  return;
                        //  Agent.speed = move.MovementSpeed;
                    }



                    if (Agent.hasPath)
                    {
                        float dist = move.DistanceRemaining = Vector3.Distance(move.TargetLocation, Agent.transform.position);

                        if (dist < move.StoppingDistance)
                        {
                            // need to improve logic for picking a location to stand at 
                            move.CanMove = false;
                            Agent.isStopped = true;
                            Agent.ResetPath();
                            move.Completed = true;
                        }
                    }
                }
                else
                {
                    Agent.isStopped = true;

                }


            });


        }
    }

}