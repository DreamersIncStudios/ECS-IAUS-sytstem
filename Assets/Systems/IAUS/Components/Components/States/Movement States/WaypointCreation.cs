﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Utilities;
using Global.Component;
using log4net.Util;
using Unity.Transforms;

namespace IAUS.ECS.Component
{
    public partial class WaypointCreationSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            foreach (var (buffer, transform, patrol, tag) in SystemAPI.Query<DynamicBuffer<TravelWaypointBuffer>, WorldTransform, Patrol, SetupBrainTag>())
            {
                List<TravelWaypointBuffer> Waypoints = GetPoints(transform.Position, 100, patrol.NumberOfWayPoints, true);
                foreach (var item in Waypoints)
                {
                    buffer.Add(item);
                }
            }
            foreach (var (buffer, transform, traverse, tag) in SystemAPI.Query<DynamicBuffer<TravelWaypointBuffer>, WorldTransform, Traverse, SetupBrainTag>())
            {
                List<TravelWaypointBuffer> Waypoints = GetPoints(transform.Position, 100, traverse.NumberOfWayPoints, true);
                foreach (var item in Waypoints)
                {
                    buffer.Add(item);
                }
            }
        }
        List<TravelWaypointBuffer> GetPoints(float3 start, uint range, uint NumOfPoints, bool Safe)
        {

            List<TravelWaypointBuffer> Points = new List<TravelWaypointBuffer>();
            while (Points.Count < NumOfPoints)
            {
                if (GlobalFunctions.RandomPoint(start, range, out Vector3 position))
                {
                    Points.Add(new TravelWaypointBuffer()
                    {
                        WayPoint = new Waypoint()
                        {
                            Position = (float3)position,
                            Point = new AITarget()
                            {
                                Type = TargetType.Location,
                                //FactionID = -1
                            },

                            TimeToWaitatWaypoint = UnityEngine.Random.Range(5, 10)
                        }
                    }
                 );
                }
            }
            return Points;

        }
    }

 
}