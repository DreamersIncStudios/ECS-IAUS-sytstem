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
        public override void Execute()
        {
            if (!SetTimer) {
                Timer = Agent.TimeAtLoc;
                SetTimer = !SetTimer;
            }
            if (Timer > 0.0f)
            {
                Timer -= Time.deltaTime*IntervalOffset;

            }
       

            //Reset Timer when new point is set;
        }

        public override void Score()
        {

            base.Score();
        }

        public override void Setup()
        {
            Considerations = new List<ConsiderationBase>();
            Considerations.Add(new WaitConsideration { NameId = "WaitAtPoint", Agent = Agent, Inverse = false });
            Considerations.Add(new DistanceTO { NameId = "Distance To Point", Agent = Agent, Inverse = true});
            Considerations.Add(new PlayerInSight { NameId = "Can I see Player" });

        }



    }
}