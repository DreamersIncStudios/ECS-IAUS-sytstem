using Unity.Entities;
using Global.Component;
using System;
using DreamersIncStudio.FactionSystem;
using DreamersIncStudio.GAIACollective;
using IAUS.ECS.StateBlobSystem;

namespace IAUS.ECS.Component
{



    [Serializable]
    public struct IAUSBrain : IComponentData
    {
        public AITarget Target;
        public AIStates CurrentState;
        public FactionNames FactionID;
        public Status Attitude;
        public Difficulty Difficulty;
        public NPCLevel NPCLevel;
        public BlobAssetReference<AIStateBlobAsset> State;
        public Role Role;
    }
    public struct SetupBrainTag : IComponentData { }
    
    public enum Status { Normal, Brave, Reckless, Berserk, Cautious, Sleep, Confused, Dazed }
}