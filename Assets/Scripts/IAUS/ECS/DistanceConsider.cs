using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.ECS.Consideration;

[System.Serializable]
public struct DistanceConsider : IConsideration
{
    public float Score { get; set; }
    public ResponseType responseType { get; set; }
    public float M { get; set; }
    public float K { get; set; }
    public float B { get; set; }
    public float C { get; set; }
    public void Consider()
    {
        throw new System.NotImplementedException();
    }

    public float Output(float input)
    {
        float temp = new float();
        switch (responseType)
        {
            case ResponseType.LinearQuad:
                temp = M * Mathf.Pow((input - C), K) + B;
                break;
            case ResponseType.Log:
                temp = K * (1.0f / (1.0f + Mathf.Pow((1000.0f * M * Mathf.Exp(1)), -input + C))) + B;
                break;
            case ResponseType.Logistic:
                temp = K * (1.0f / (1.0f + Mathf.Pow((1000.0f * M * Mathf.Exp(1)), -input + C))) + B;
                break;
        }
        return temp;
    }
}
