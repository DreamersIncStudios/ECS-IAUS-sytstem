using Unity.Entities;
using Global.Component;
using System;
using DreamersIncStudio.FactionSystem;
using DreamersIncStudio.GAIACollective;
using IAUS.ECS.StateBlobSystem;
using Unity.Collections;

namespace IAUS.ECS.Component
{



    [Serializable]
    public struct IAUSBrain : IComponentData
    {
        public FixedList128Bytes<StateData> StatesToCheck;
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
    [Serializable]
    public struct StateData
    {
        public readonly AIStates State;
        public StateData(AIStates state)
        {
         State = state;
         Index = -1;
         Status = ActionStatus.Idle;
        }
        public int Index;
        public ActionStatus Status{ get; private set; }
        public void SetIndex(int index)
        {
            Index = index;
        }
        public void SetStatus(ActionStatus status)
        {
            Status = status;
        }
    }
}