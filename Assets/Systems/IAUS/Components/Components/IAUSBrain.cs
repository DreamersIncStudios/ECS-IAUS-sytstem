using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Global.Component;
using DreamersInc.InflunceMapSystem;
using System;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
using UnityEngine.Serialization;

namespace IAUS.ECS.Component
{



    [Serializable]
    public struct IAUSBrain : IComponentData
    {
        public AITarget Target;
        public AIStates CurrentState;
         public int FactionID;// ID CHanging?
        public Status Attitude;
        public Difficulty Difficulty;
        public NPCLevel NPCLevel;
        public BlobAssetReference<AIStateBlobAsset> State;
    }
    public struct SetupBrainTag : IComponentData { }
    
    public enum Status { Normal, Brave, Reckless, Berserk, Cautious, Sleep, Confused, Dazed }
}