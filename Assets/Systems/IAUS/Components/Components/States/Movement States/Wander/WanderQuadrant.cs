using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;
using DreamersInc.QuadrantSystems;


namespace IAUS.ECS.Component
{
  
    public struct WanderActionTag : IComponentData
    {
        public float WaitTime;
        public TravelPlan Plan;
        public Waypoint CurWaypoint { get; set; }
        public float WaitTimer; // Value base TBDs
        public float3 TravelPosition;
        public int HashKey;
    }
    [System.Serializable]
    public enum TravelPlan
    {
        none, 
        GetNewLocation,
        MoveToLocation, 
        Wait
    }
 
}