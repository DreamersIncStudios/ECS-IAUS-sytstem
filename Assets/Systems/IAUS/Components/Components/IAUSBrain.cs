﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Global.Component;
using DreamersInc.InflunceMapSystem;
using System;
using IAUS.ECS.Consideration;
namespace IAUS.ECS.Component
{



    [Serializable]
    public struct IAUSBrain : IComponentData
    {
        public AITarget Target;
        public AIStates CurrentState;
        public int factionID; // ID CHanging?
        public Status Attitude;
        public Difficulty Difficulty;
        public NPCLevel NPCLevel;
    }
    public struct SetupBrainTag : IComponentData { }

    public struct StateBuffer : IBufferElementData
    {
       [SerializeField] public AIStates StateName { get; private set; }
        public float TotalScore;
        public ActionStatus Status;
        public bool ConsiderScore => Status == ActionStatus.Idle || Status == ActionStatus.Running;

        public StateBuffer(AIStates state) {
            StateName = state;
            Status = ActionStatus.Idle;
            TotalScore = 0;
        }

    }
    public enum Status { Normal, Brave, Reckless, Berserk, Cautious, Sleep, Confused, Dazed }
}