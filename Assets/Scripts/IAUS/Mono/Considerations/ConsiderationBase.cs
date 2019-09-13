using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IAUS
{
    [System.Serializable]
    public abstract class ConsiderationBase : IConsideration
    {
        public string NameId { get; set; }
        public bool Inverse { get; set; } // Is Invense Required if m is negatives? Inverse to be removed for response curves
        public float Score { get; set; }
        public CharacterContext Agent { get; set; }
        public abstract void Consider();
        public ResponseType responseType { get; set; }
        public float M { get; set; }
        public float K { get; set; } // Value of K is to be between -1 and 1 for Logistic Responses
        public float B { get; set; }
        public float C { get; set; }
        public float Output(float input) {
            float temp = new float();
            switch (responseType) {
                case ResponseType.LinearQuad:
                    temp = M * Mathf.Pow((input - C), K) + B;
                    break;
                case ResponseType.Log:
                    temp = K*(1.0f/(1.0f+Mathf.Pow((1000.0f * M * Mathf.Exp(1)), -input + C)))+B;
                    break;
                case ResponseType.Logistic:
                    temp = K * (1.0f / (1.0f + Mathf.Pow((1000.0f * M * Mathf.Exp(1)), -input + C))) + B;
                    break;
            }
            return temp;
        }

        // public abstract IConsideration Clone();


    }
}
