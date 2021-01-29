using Unity.Entities;
using UnityEngine;
namespace Global.Component
{
    [System.Serializable]
    [GenerateAuthoringComponent]
    public struct AITarget : IComponentData
    {
        public TargetType Type;
        public int NumOfEntityTargetingMe;
        public bool CanBeTargeted => NumOfEntityTargetingMe < 2;
        [HideInInspector] public int MaxNumberOfTarget; // base off of Threat Level
        public bool CanBeTargetByPlayer;


    }
    [System.Serializable]
    public enum TargetType
    {
        None, Character, Location, Vehicle
    }
  

}