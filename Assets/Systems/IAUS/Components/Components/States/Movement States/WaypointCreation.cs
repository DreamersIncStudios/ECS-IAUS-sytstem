using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Utilities;
using Global.Component;
using Unity.Transforms;
using IAUS.ECS.StateBlobSystem;
using Random = UnityEngine.Random;
using Unity.Burst;
using DreamersInc.QuadrantSystems;

namespace IAUS.ECS.Component
{
    [UpdateBefore(typeof(SetupAIStateBlob))]
    public partial struct WaypointCreationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {

            foreach (var (buffer, transform, patrol, tag) in SystemAPI.Query<DynamicBuffer<TravelWaypointBuffer>, LocalTransform, Patrol, SetupBrainTag>())
            {
                List<TravelWaypointBuffer> Waypoints = new List<TravelWaypointBuffer>();
                if (!patrol.StayInQuadrant)
                {
                    Waypoints = GetPoints(transform.Position, 200, patrol.NumberOfWayPoints, true);
                }
                else {
                    Waypoints = GetPointsInQuadrant(transform.Position, 200, patrol.NumberOfWayPoints);
                }

                foreach (var item in Waypoints)
                {
                    buffer.Add(item);
                }
            }
            foreach (var (buffer, transform, traverse, tag) in SystemAPI.Query<DynamicBuffer<TravelWaypointBuffer>, LocalTransform, Traverse, SetupBrainTag>())
            {
                List<TravelWaypointBuffer> Waypoints = GetPoints(transform.Position, 400, traverse.NumberOfWayPoints, true);
                foreach (var item in Waypoints)
                {
                    buffer.Add(item);
                }
            }

        }
        List<TravelWaypointBuffer> GetPoints(float3 start, uint range, uint NumOfPoints, bool Safe)
        {

            List<TravelWaypointBuffer> Points = new();
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

                            TimeToWaitatWaypoint = Random.Range(5, 25)
                        }
                    }
                 );
                }
            }
            return Points;

        }
        List<TravelWaypointBuffer> GetPointsInQuadrant(float3 start, uint range, uint NumOfPoints)
        {

            List<TravelWaypointBuffer> Points = new();
            int hashKey = NPCQuadrantSystem.GetPositionHashMapKey((int3)start);
            while (Points.Count < NumOfPoints)
            {
                if (GlobalFunctions.RandomPoint(start, range, out float3 position))
                {
                    if(hashKey == NPCQuadrantSystem.GetPositionHashMapKey((int3)position))
                    Points.Add(new TravelWaypointBuffer()
                    {
                        WayPoint = new Waypoint()
                        {
                            Position = position,
                            Point = new AITarget()
                            {
                                Type = TargetType.Location,
                                //FactionID = -1
                            },

                            TimeToWaitatWaypoint = Random.Range(5, 25)
                        }
                    }
                 );
                }
            }
            return Points;

        }
    }


}