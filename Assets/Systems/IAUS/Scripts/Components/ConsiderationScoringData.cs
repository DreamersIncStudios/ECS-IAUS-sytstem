using UnityEngine;
using System;
using Unity.Collections;

namespace IAUS.ECS.Consideration
{ 

    [Serializable]
    public struct ConsiderationScoringData
    {
        public bool Inverse; // Is Invense Required if m is negatives? Inverse to be removed for response curves
        public ResponseType responseType;
        // add getter setters
        public float M;
        public float K; // Value of K is to be between -1 and 1 for Logistic Responses
        public float B;
        public float C;

        public float Output(float input)
        {
            if (input > 1.0f || input < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(input), $"Input outside of bounds of expection input value: {input}");
            }

            float temp = new float();
            switch (responseType)
            {
                case ResponseType.LinearQuad:
                    if(!Inverse)
                    temp = M * Mathf.Pow((input - C), K) + B;
                    else
                        temp = 1 - M * Mathf.Pow((input - C), K) + B;

                    break;
                case ResponseType.Log:
                    temp = K * (1.0f / (1.0f + Mathf.Pow((1000.0f * M * Mathf.Exp(1)), input - C))) + B;
                    break;
                case ResponseType.Logistic:
                    temp = K * (1.0f / (1.0f + Mathf.Pow((1000.0f * M * Mathf.Exp(1)), input - C))) + B;
                    break;
            }
            return Mathf.Clamp01(temp);
        }
    }
        public enum ResponseType
        {
            none,
            LinearQuad,
            Log,
            Logistic

        }

    
}