using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.ECS.Consideration;
using System;

namespace IAUS.ECS.Consideration
{
    [Serializable]
    public struct ConsiderationBaseECS : IConsideration
    {
        public float Score { get; set; }
        public ResponseTypeECS responseType { get; set; }
        public float M { get; set; }
        public float K { get; set; }
        public float B { get; set; }
        public float C { get; set; }

        public float Output(float input)
        {
            float temp = new float();
            switch (responseType)
            {
                case ResponseTypeECS.LinearQuad:
                    temp = M * Mathf.Pow((input - C), K) + B;
                    break;
                case ResponseTypeECS.Log:
                    temp = K * (1.0f / (1.0f + Mathf.Pow((1000.0f * M * Mathf.Exp(1)), -input + C))) + B;
                    break;
                case ResponseTypeECS.Logistic:
                    temp = K * (1.0f / (1.0f + Mathf.Pow((1000.0f * M * Mathf.Exp(1)), -input + C))) + B;
                    break;
            }
            return temp;
        }
    }
}