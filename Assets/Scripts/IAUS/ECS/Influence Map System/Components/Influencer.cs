using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace InfluenceMap
{
    [GenerateAuthoringComponent]
    public struct Influencer : IComponentData
    {
        public float Influence;
        public Threat threat;
        public Threat Protection;
        public int Range;
        public FallOff fallOff;

        public float M { get { return 2.0f; } }
        public float K { get { return 1.0f; } } // Value of K is to be between -1 and 1 for Logistic Responses
        public float B { get { return 0.0f; } }
        public float C { get { return 0.0f; } }

        public float Output(float input)
        {
            float temp = new float();
            switch (fallOff)
            {
                case FallOff.linear:
                    temp = M * Mathf.Pow((input - C), K) + B;
                    break;
                case FallOff.Quadratic:
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
    public struct Threat
    {
        public float Global;
        public float Player, Enemy;// To Be Expanded later
    }
    public enum FallOff
    {
        linear,
        Quadratic,
        Inverse,
        Ring,
        Barrier,
        Logistic


    }



}