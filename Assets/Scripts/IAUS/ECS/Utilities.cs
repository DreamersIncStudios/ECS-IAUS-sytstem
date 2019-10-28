using UnityEngine;
using System;
namespace IAUS.ECS.Utilities {


    public interface IConsideration 
    {
        [SerializeField]float Score { get; set; }
        //      bool Inverse { get; set; } Replace using Response curve Slope
        ResponseTypeECS responseType { get; set; }
        float Output(float input);
        float M { get; set; }
        float K { get; set; }
        float B { get; set; }
        float C { get; set; }

    }
    public enum ResponseTypeECS
    {
        LinearQuad,
        Log,
        Logistic

    }
    [Serializable]
    public struct ConsiderationBased : IConsideration
    {
       [SerializeField] public float Score { get; set; }
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
                    temp = K * (1.0f / (1.0f + Mathf.Pow((1000.0f * M * Mathf.Exp(1)), input - C))) + B;
                    break;
                case ResponseTypeECS.Logistic:
                    temp = K * (1.0f / (1.0f + Mathf.Pow((1000.0f * M * Mathf.Exp(1)), +input - C))) + B;
                    break;
            }
            return temp;
        }
    }
}