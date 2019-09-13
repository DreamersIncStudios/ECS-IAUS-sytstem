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
                responseType = ResponseType.Logistic,
                M = 50,
                K = .85f,
                B = 0.15f,
                C = .6f
            });
          // Considerations.Add(new PlayerInSight { NameId = "Can I see Player" });
           Considerations.Add(new WaitConsideration { NameId = "WaitAtPoint", Agent = Agent    ,                         
               responseType = ResponseType.Logistic,
               M = 5,
               K = -.99f,
               B = 1.0f,
               C = 0.6f
           });

        }
        public override void OnStart()
        {
            throw new System.NotImplementedException();
        }
        public override void OnUpdate()
        {

            Agent.NavAgent.SetDestination(Agent.PointOfInterest.position);
            float distFromPOI = Vector3.Distance(Agent.transform.position, Agent.PointOfInterest.position);
            if (distFromPOI < 1.5f) { OnExit(); }

        }
        public override void OnExit()
        {
            Agent.Timer = Agent.TimeAtLoc;
            Agent.ExitTimerLoop = true;
        }
    }

}