using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Utilities;
namespace IAUS.ECS2.Component
{
    public class WaypointCreation : MonoBehaviour, IConvertGameObjectToEntity
    {
        List<PatrolWaypointBuffer> GetPoints
        {
            get
            {
                List<PatrolWaypointBuffer> Points = new List<PatrolWaypointBuffer>();
                while (Points.Count < 10)
                {
                    if (GlobalFunctions.RandomPoint(transform.position, 60, out Vector3 position))
                    {
                        Points.Add(new PatrolWaypointBuffer()
                        {
                            WayPoint = new Waypoint()
                            {
                                Point = new AITarget() 
                                {
                                    Position= (float3)position },
                                TimeToWaitatWaypoint = UnityEngine.Random.Range(10,20)
                            }
                        }
                     );
                    }
                }
                return Points;
            }
        }
       

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            DynamicBuffer<PatrolWaypointBuffer> buffer = dstManager.AddBuffer<PatrolWaypointBuffer>(entity);

          List<PatrolWaypointBuffer>  Waypoints = GetPoints;
            foreach (var item in Waypoints)
            {
                buffer.Add(item);
            }
        }
    }
}