using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace IAUS.ECS2
{
    public struct StateBuffer : IBufferElementData
    {
        public AIStates StateName;
        public float TotalScore;
        public ActionStatus Status;

    }
}