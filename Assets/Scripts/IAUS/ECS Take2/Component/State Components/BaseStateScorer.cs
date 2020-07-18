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
        float mod { get; }
    }

    public enum ActionStatus   
    {
         Success, Running, Interrupted, Idle, CoolDown, Disabled, Failure
    }
}
