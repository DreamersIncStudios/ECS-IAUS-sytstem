using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using Global.Component;
using AISenses;
using Components.MovementSystem;
using IAUS.ECS.Component;
using DreamersInc.InflunceMapSystem;
using UnityEngine.AI;
using Unity.Entities;
using Unity.Transforms;
using Utilities;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using System.Linq;
using Unity.Physics;
using Unity.Physics.Authoring;

public  class NPCSpawn : MonoBehaviour
{
    static List<GameObject> Models;
    public static PhysicsCategoryTags belongsTo;
    public static PhysicsCategoryTags collideWith;
    bool loaded;
    public void LoadModals()
    {
        GameObject[] goLoaded = Resources.LoadAll("Players", typeof(GameObject)).Cast<GameObject>().ToArray();
        foreach (var go in goLoaded)
        {
            Models.Add(go);
        }
        loaded = true;
    }

   

    static List<TravelWaypointBuffer> GetPoints(uint range, uint NumOfPoints, Vector3 pos, bool Safe = true)
    {

        List<TravelWaypointBuffer> Points = new List<TravelWaypointBuffer>();
        while (Points.Count < NumOfPoints)
        {
            if (GlobalFunctions.RandomPoint(pos, range, out Vector3 position))
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
}
