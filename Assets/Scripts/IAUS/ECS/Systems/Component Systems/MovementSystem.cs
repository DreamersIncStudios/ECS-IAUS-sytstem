
using UnityEngine;
using UnityEngine.AI;
using Unity.Entities;
using IAUS.ECS.Component;
using Unity.Transforms;
using InfluenceMap;


namespace IAUS.ECS.System
{

    public class MovementSystem : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((NavMeshAgent Agent,ref Movement move, ref LocalToWorld toWorld, ref InfluenceValues  influenceValues ) => {
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
                        float dist = move.DistanceRemaining = Vector3.Distance(Agent.destination, Agent.transform.position);
                      
                        if (dist <= 1.0f)
                        {
                            // need to improve logic for picking a locatio to stand at 
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