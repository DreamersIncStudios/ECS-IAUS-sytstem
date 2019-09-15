//Copyright 2019 <Dreamers Inc Studios>
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.




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
        public override void Excute()
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
            base.Setup();
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