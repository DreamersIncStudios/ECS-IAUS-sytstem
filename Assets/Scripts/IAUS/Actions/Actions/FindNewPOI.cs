using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.Considerations;

namespace IAUS.Actions
{
    public class FindNewPOI : ActionBase
    {
        int WaypointIndex=0;
        List<Transform> Waypoint { get { return Agent.Waypoints; } }
        public override void Execute()
        {
            if (WaypointIndex < Waypoint.Count-1)
                WaypointIndex++;
            else WaypointIndex = 0;

            Agent.PointOfInterest = Agent.Waypoints[WaypointIndex];
        }

        public override void Setup()
        {
            Considerations = new List<ConsiderationBase>();
            Considerations.Add(new DistanceTO
            {
                NameId = "Distance To Next Waypoint",
                Agent = Agent,
                Inverse = true,
            });
            Considerations.Add(new PlayerInSight { NameId = "Can I see Player" });
            Considerations.Add(new WaitConsideration { NameId = "WaitAtPoint", Agent = Agent, Inverse = true });

        }

    }
}