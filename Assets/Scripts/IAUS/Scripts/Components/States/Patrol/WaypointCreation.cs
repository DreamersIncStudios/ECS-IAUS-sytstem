using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Utilities;
using Global.Component;

namespace IAUS.ECS.Component
{
    public class WaypointCreation : MonoBehaviour, IConvertGameObjectToEntity
    {

        /// <summary>
        /// Need to rewrite to get gameobject in world with this AITarget tag at runtime. Consider making a component system or job;
        /// </summary>
        public uint NumberOfWayPoints = 5;

        List<PatrolWaypointBuffer> GetPoints
        {
            get
            {
                List<PatrolWaypointBuffer> Points = new List<PatrolWaypointBuffer>();
                while (Points.Count < NumberOfWayPoints)
                {
                    if (GlobalFunctions.RandomPoint(transform.position, 400, out Vector3 position))
                    {
                        Points.Add(new PatrolWaypointBuffer()
                        {
                            WayPoint = new Waypoint()
                            {
                                Position = (float3)position,
                                Point = new AITarget() 
                                {
                                    Type = TargetType.Location,
                                    GetRace = Race.None
                                },
                                
                                TimeToWaitatWaypoint = UnityEngine.Random.Range(5,10)
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