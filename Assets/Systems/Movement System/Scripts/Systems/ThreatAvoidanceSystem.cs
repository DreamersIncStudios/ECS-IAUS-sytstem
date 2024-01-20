/*
using DreamersInc.InflunceMapSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

namespace DreamersInc.MovementSys
{
    public partial class ThreatAvoidanceSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((NavMeshAgent agent, ref InfluenceComponent influence, ref LocalTransform transform) => {
                Vector3 posByNextUpdate = PositionOnPathByDistance(agent);
               var influenceAtNextPos = InfluenceGridMaster.Instance.grid.GetGridObject(transform.Position).GetAverageValue((influence.factionID));
                if (influenceAtNextPos.x > 3) {
                    Debug.Log("I need to Leave");
                
                }
            }).Run();
        }

        Vector3 PositionOnPathByDistance(NavMeshAgent agent, float dist)
        {
            if (agent.hasPath && agent.remainingDistance < dist && agent.remainingDistance > 5.0)
            {
                Vector3[] pathCorners = agent.path.corners;
                int closestIndex = 0;
                float closestDistance = Vector3.Distance(agent.transform.position, pathCorners[0]);

                for (int i = 1; i < pathCorners.Length; i++)
                {
                    float distanceToCorner = Vector3.Distance(agent.transform.position, pathCorners[i]);
                    if (distanceToCorner < closestDistance)
                    {
                        closestDistance = distanceToCorner;
                        closestIndex = i;
                    }
                }
                return pathCorners[closestIndex];
            }
            else
            {
                return -Vector3.one;
            }
        }



        Vector3 PositionOnPathByDistance(NavMeshAgent agent)
        {
            return PositionOnPathByDistance(agent, agent.speed * 3);
        }
    }
}
*/
