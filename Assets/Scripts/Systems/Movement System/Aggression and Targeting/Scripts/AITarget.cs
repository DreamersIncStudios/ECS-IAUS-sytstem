using Unity.Entities;
using UnityEngine;
using DreamersInc.InflunceMapSystem;
namespace Global.Component
{
    [System.Serializable]
    /// Do not add [GenerateAuthoring] tag use AITargetCreate
    public struct AITarget : IComponentData
    {
        public TargetType Type;
        public int FactionID;
        public int NumOfEntityTargetingMe;
        [HideInInspector] public int GetInstanceID;
        public bool CanBeTargeted => NumOfEntityTargetingMe < 2;
        [HideInInspector] public int MaxNumberOfTarget; // base off of Threat Level
        public bool CanBeTargetByPlayer;
        public bool IsFriend(int factionID) {
            bool test = new bool();
       
            
            return test; }


    }
    [System.Serializable]
    public enum TargetType
    {
        None, Character, Location, Vehicle
    }


}