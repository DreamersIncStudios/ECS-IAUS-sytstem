using IAUS.ECS2;
using System;

namespace ProjectRebirth.Bestiary.Interfaces
{
    public interface BaseAIState {

        float ResetTime { get; }

    }
    public interface iPatrol : BaseAIState {
        ConsiderationData FullHealth { get; }
        ConsiderationData DistanceToTarget { get; }
        float BufferZone { get; }
        int MaxInfluenceAtPoint { get; }

    }
    public interface iWait : BaseAIState
    {
        float TimeToWait { get; }

        ConsiderationData FullHealth { get; }
        ConsiderationData WaitTimer { get; }
    }

    public interface iFollow : BaseAIState
    {
        float DistanceToMantainFromTarget { get; }
    }
    public interface IHealSelf : BaseAIState {
        ConsiderationData HealthTilDeath { get; }
        ConsiderationData TimeBetweenHeals { get; }
         ConsiderationData RecoveryItemsInInventory { get; }

        // To be Implement Later
       // ConsiderationData ThreatInArea { get; }
        int FullInventoryofItem { get; }
        

        float timeBetweenHeals { get; }
    }


    public interface iAttack : BaseAIState { }
    public interface iInvesigate : BaseAIState { }
    public interface iDetect : BaseAIState { }

    public interface AIDriven { 
        ActiveAIStates activeAIStates { get; }
    }

    [Serializable]
    public struct ActiveAIStates {
        public bool Patrol;
        public bool Follow;
        public bool Wait;
        public bool Attack;
        public bool Investiage;
        public bool Detection;
        public bool HealSelfViaItem;
        public bool HealSelfViaMagic;



    }

}
