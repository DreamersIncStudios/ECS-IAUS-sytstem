using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using IAUS.Considerations;

namespace IAUS.Actions
{
    [System.Serializable]
    public class PatrolArea : ActionBase
    {
        public List<Transform> Waypoints;
        float TimeAtLoc { get { return Agent.TimeAtLoc; } }
        public override void Setup()
        {
            Considerations = new List<ConsiderationBase>();
            Considerations.Add(new DistanceTO {
                NameId = "Distance To Next Waypoint",
                Agent = Agent,
                Inverse= false, 
            });
           Considerations.Add(new PlayerInSight { NameId = "Can I see Player" });
           Considerations.Add(new WaitConsideration { NameId = "WaitAtPoint", Agent = Agent ,Inverse = true});

        }

        public override void Execute()
        {

            Agent.NavAgent.SetDestination(Agent.PointOfInterest.position);

        }
    }

}