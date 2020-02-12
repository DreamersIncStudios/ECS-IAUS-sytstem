using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace IAUS.ECS2 {

    public interface BaseStateScorer : IComponentData
    {
        //should this be in a struct IBuffer??
        float TotalScore { get; set; }
        ActionStatus Status { get; set; }
        float ResetTimer { get; set; }
        float ResetTime { get; set; }
    }

    public enum ActionStatus   
    {
        Failure, Succes, Running, Interrupted, Idle
    }

    public struct StateBuffer : IBufferElementData {
        public AIStates StateName;
        float TotalScore { get; set; }
        ActionStatus Status { get; set; }

    }
}
