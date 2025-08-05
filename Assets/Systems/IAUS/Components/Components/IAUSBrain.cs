using Unity.Entities;
using Global.Component;
using System;
using DreamersIncStudio.GAIACollective;
using IAUS.ECS.StateBlobSystem;

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
        public Role Role;
    }
    public struct SetupBrainTag : IComponentData { }
    
    public enum Status { Normal, Brave, Reckless, Berserk, Cautious, Sleep, Confused, Dazed }
}