using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAUS.Considerations {
    public class DistanceTO : ConsiderationBase
    {
        public float MaxRange = 75.0f;
        public float MinRange = 3.0f;
        //public IConsideration Clone()
        //{
        //    throw new System.NotImplementedException();
        //}
        public Transform Point { get { return Agent.PointOfInterest; } }
        public GameObject NavAgent { get { return Agent.gameObject; } }

        public override void Consider()
        {

            //Inverse Function the closer agent is to objects value approaches zero
            Score = new float();
            float dist = Vector3.Distance(Point.position, NavAgent.transform.position);
            if (Inverse)
            {
                Score = 1.0f - Mathf.Clamp01((float)(dist - MinRange) / (float)(MaxRange - MinRange));
            }
            else
            {
                Score = Mathf.Clamp01((float)(dist - MinRange) / (float)(MaxRange - MinRange));
            }


        }

        public override void Output(float input)
        {
        }

    }
}
