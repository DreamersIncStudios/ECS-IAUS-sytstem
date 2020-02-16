using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace IAUS.ECS2
{
    public struct HealthConsideration:IComponentData {
        public float Ratio;
    }

    public struct DistanceToConsideration : IComponentData
    {
        public float Ratio;
        public float DistanceAtTimeAssigned;
        public float DistanceRemaining;
    }
    public struct TimerConsideration : IComponentData
    {
        public float Ratio;
    }
    [System.Serializable]
    public struct ConsiderationData {
        public bool Inverse; // Is Invense Required if m is negatives? Inverse to be removed for response curves
        public ResponseType responseType;
        public float M;
        public float K; // Value of K is to be between -1 and 1 for Logistic Responses
        public float B;
        public float C;
        public float Output(float input)
        {
            float temp = new float();
            switch (responseType)
            {
                case ResponseType.LinearQuad:
                    temp = M * Mathf.Pow((input - C), K) + B;
                    break;
                case ResponseType.Log:
                    temp = K * (1.0f / (1.0f + Mathf.Pow((1000.0f * M * Mathf.Exp(1)), input - C))) + B;
                    break;
                case ResponseType.Logistic:
                    temp = K * (1.0f / (1.0f + Mathf.Pow((1000.0f * M * Mathf.Exp(1)), input - C))) + B;
                    break;
            }
            return temp;
        }
    }

    public enum AIStates {
        none, Patrol, Heal_Item, Heal_Magic, Attack_Melee, Attack_Range,
        Attack_Magic, Retreat, FindCover,Talk, Guard, GroupUp, Wait, Rally, 
    }
}