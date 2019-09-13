using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAUS.Considerations
{
    public class WaitConsideration : ConsiderationBase
    {

        public float Timer { get { return Agent.Timer; } }
        public float AlloctedTime { get { return Agent.TimeAtLoc; } }


        public override void Consider()
        {
            if (Timer <= 0.0f)
            {
                Score = Mathf.Clamp01(Output(0));
                
            }
            else
            {
                Score = Mathf.Clamp01(Output(1));
                
            }


        }



    }
}