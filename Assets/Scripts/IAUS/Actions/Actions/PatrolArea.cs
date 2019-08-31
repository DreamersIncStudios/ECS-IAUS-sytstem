using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace IAUS.Actions
{
    public class PatrolArea : ActionBase
    {
        public NavMeshAgent Agent;
        public List<Transform> Waypoints;
        public float TimeAtLoc;
        public override void Setup()
        {
            Considerations = new List<ConsiderationBase>();
            Considerations.Add(new DistanceTO {
                NameId = "Distance To Next Waypoint",
                Agent =Agent.gameObject,Point=Waypoints[0].gameObject,
                Inverse=true, 
            });
        }
        public override void Score()
        {
            foreach (ConsiderationBase consideration in Considerations) {
                consideration.Consider();
            }
        }
        public override void Execute()
        {
            throw new System.NotImplementedException();
        }


    }
    public class WaitAtSpot: ActionBase
    {
        public NavMeshAgent Agent;
        public List<Transform> Waypoints;
        public float TimeAtLoc;
        public override void Setup()
        {
            Considerations = new List<ConsiderationBase>();
            Considerations.Add(new DistanceTO
            {
                NameId = "Distance To Next Waypoint",
                Agent = Agent.gameObject,
                Point = Waypoints[0].gameObject,
                Inverse = false,
            });
        }
        public override void Score()
        {
            foreach (ConsiderationBase consideration in Considerations)
            {
                consideration.Consider();
            }
        }
        public override void Execute()
        {
            throw new System.NotImplementedException();
        }


    }
}