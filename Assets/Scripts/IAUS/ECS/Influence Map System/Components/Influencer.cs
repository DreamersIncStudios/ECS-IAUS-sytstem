using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace InfluenceMap
{
    [GenerateAuthoringComponent]
    [System.Serializable]
    public struct Influencer : IComponentData
    {
        public Influence influence;
        public int Range;
        public float RingWidth;
        public FallOff fallOff { get; set; }



        public float M { get { return 3.0f; } }
        public float K { get { return 1.0f; } } // Value of K is to be between -1 and 1 for Logistic Responses
        public float B { get { return -2.0f; } }
        public float C { get { return 0.0f; } }

        public float Output(float input)
        {
            float temp = new float();
            switch (fallOff)
            {
                case FallOff.LinearQuadInverse:
                    temp = M * Mathf.Pow((input - C), K) + B;
                    break;
                case FallOff.Logistic:
                    temp = K * (1.0f / (1.0f + Mathf.Pow((1000.0f * M * Mathf.Exp(1)), -input + C))) + B;
                    break;
            }
            return temp;
        }

    }
    [System.Serializable]
    public struct Influence {
      
        public float2 Proximity;   //In physical attack range  Value and range
        public float2 Threat; //In range of attack or notice   Value and range
    }

    public enum FallOff
    {
        LinearQuadInverse,
        Ring,
        Barrier,
        Logistic


    }



}