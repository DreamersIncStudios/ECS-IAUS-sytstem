using System.Collections;
using System.Collections.Generic;
using IAUS.ECS.Utilities;
using Unity.Entities;
using Unity.Mathematics;
using IAUS.ECS.Component;
using UnityEngine;

public class Cop : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
namespace IAUS.ECS.Component
{
    public struct Police: IComponentData
    {
        public float3 HomePos;
        public int TravelRange;
        public int CashOnHand;
        public int MaxCashOnHand;
        public int boughtItem;
        public int CarryLimit;
        public bool Arrest;

    }
}