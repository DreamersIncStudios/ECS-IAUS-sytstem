using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.Considerations;

namespace IAUS.Actions
{
    public class WaitAtPoint : ActionBase
    {

        float Timer { get { return Agent.Timer; }set { Agent.Timer = value; } }
        public float AlloctedTime;
        bool SetTimer;
        public int IntervalOffset;
        List<Transform> Waypoint { get { return Agent.Waypoints; } }
        int WaypointIndex = 0;

        public override void OnStart()
        {
            throw new System.NotImplementedException();
        }
        public override void OnUpdate()
        {

            if (Timer > 0.0f)
            {
                Timer -= Time.deltaTime * IntervalOffset;
            }
            if(Timer>.1f && Agent.ExitTimerLoop) {

                OnExit();

                Agent.ExitTimerLoop = false;
            }
       

            //Reset Timer when new point is set;
        }

        public override void OnExit()
        {
            if (WaypointIndex < Waypoint.Count - 1)
                WaypointIndex++;
            else WaypointIndex = 0;

            Agent.PointOfInterest = Agent.Waypoints[WaypointIndex];
        }
        public override void Score()
        {

            base.Score();
        }

        public override void Setup()
        {
            Considerations = new List<ConsiderationBase>();
            Considerations.Add(new WaitConsideration { NameId = "WaitAtPoint", Agent = Agent,
                            responseType = ResponseType.Logistic,
                M = 50,
                K = .95f,
                B = 0.05f,
                C = 0.6f
            });
            Considerations.Add(new DistanceTO { NameId = "Distance To Point", Agent = Agent, MinRange=0,
                responseType = ResponseType.Logistic, M = 50,K=-.95f,B=1,C=.6f
 });
           // Considerations.Add(new PlayerInSight { NameId = "Can I see Player" });

        }



    }
}