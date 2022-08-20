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
        public bool SafeZone;
        List<TravelWaypointBuffer> GetPoints(uint range, uint NumOfPoints, bool Safe)
        {
            
                List<TravelWaypointBuffer> Points = new List<TravelWaypointBuffer>();
            while (Points.Count < NumOfPoints)
            {
                if (GlobalFunctions.RandomPoint(transform.position, range, out Vector3 position))
                {
                    Points.Add(new TravelWaypointBuffer()
                    {
                        WayPoint = new Waypoint()
                        {
                            Position = (float3)position,
                            Point = new AITarget()
                            {
                                Type = TargetType.Location,
                                FactionID = -1
                            },

                            TimeToWaitatWaypoint = UnityEngine.Random.Range(5, 10)
                        }
                    }
                 );
                }
            }    
                return Points;
            
        }

        Entity self;
       
        public void CreateWaypoints(uint range, uint NumOfPoints, bool Safe) {
            DynamicBuffer<TravelWaypointBuffer> buffer = World.DefaultGameObjectInjectionWorld.EntityManager.AddBuffer<TravelWaypointBuffer>(self);
            List<TravelWaypointBuffer> Waypoints = GetPoints(range, NumOfPoints,Safe);
            foreach (var item in Waypoints)
            {
                buffer.Add(item);
            }
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            self = entity;

          //  DynamicBuffer<TravelWaypointBuffer> buffer = dstManager.AddBuffer<TravelWaypointBuffer>(entity);

          //List<TravelWaypointBuffer>  Waypoints = GetPoints(400, 5 ,false);
          //  foreach (var item in Waypoints)
          //  {
          //      buffer.Add(item);
          //  }
        }
    }
}