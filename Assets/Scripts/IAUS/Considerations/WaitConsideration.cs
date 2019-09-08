using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAUS.Considerations
{
    public class WaitConsideration : ConsiderationBase
    {

        public float Timer { get { return Agent.Timer; } }
        public float AlloctedTime;


        public override void Consider()
        {
     
            if (!Inverse)
            {
                if (Timer >0.2f)
                { Score = 1.0f; }
                else { Score = 0.0f; }
            }
            else
            {
                if (Timer > 0.2f)
                { Score = 0.0f; }
                else
                {
                    Score = 1.0f;
                }
            }

        }

        public override void Output(float input)
        {
        }

    }
}