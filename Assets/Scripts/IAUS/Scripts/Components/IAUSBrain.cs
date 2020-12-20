using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace IAUS.ECS2.Component
{



    [System.Serializable]
    [GenerateAuthoringComponent]
    public struct IAUSBrain : IComponentData
    {
        public AITarget Target;
        public AIStates CurrentState;
    }
    public struct SetupBrainTag : IComponentData { }

    public struct StateBuffer : IBufferElementData
    {
        public AIStates StateName;
        public float TotalScore;
        public ActionStatus Status;
        public bool ConsiderScore => Status == ActionStatus.Idle || Status == ActionStatus.Running;

    }
}