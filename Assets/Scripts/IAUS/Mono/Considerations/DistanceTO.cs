using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
namespace IAUS.Considerations {
    public class DistanceTO : ConsiderationBase
    {
        public float MaxRange = 75.0f;
        public float MinRange = 3.0f;
        //public IConsideration Clone()
        //{
        //    throw new System.NotImplementedException();
        //}
        public Transform Point;  // This need to be set by the action and a point of interest allow for my reuse
        
        GameObject NavAgent { get { return Agent.gameObject; } }

        public override void Consider()
        {

            //Inverse Function the closer agent is to objects value approaches zero
            Score = new float();
            float dist = Vector3.Distance(NavAgent.transform.position, Point.position);
                
            
            float input = Mathf.Clamp01((float)(dist- MinRange) / (float)(MaxRange - MinRange));
            Score = Mathf.Clamp01(Output(input));
       


        }

    }

    }

  
