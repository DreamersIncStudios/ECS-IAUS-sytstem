using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Global.Component;
using DreamersInc.InflunceMapSystem;
using System;
namespace IAUS.ECS.Component
{



    [Serializable]
    [GenerateAuthoringComponent]
    public struct IAUSBrain : IComponentData
    {
        public AITarget Target;
        public AIStates CurrentState;
        public Faction faction;
        public Attitude Attitude;
    }
    public struct SetupBrainTag : IComponentData { }

    public struct StateBuffer : IBufferElementData
    {
        public AIStates StateName;
        public float TotalScore;
        public ActionStatus Status;
        public bool ConsiderScore => Status == ActionStatus.Idle || Status == ActionStatus.Running;

    }
    public enum Attitude { Normal, Brave, Reckless, Berserk, Cautious, Sleep, Confused, Dazed }
}