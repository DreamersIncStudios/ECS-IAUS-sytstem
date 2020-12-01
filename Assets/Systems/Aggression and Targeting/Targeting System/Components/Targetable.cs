using Unity.Entities;
using UnityEngine;
namespace DreamersStudio.TargetingSystem
{
    [GenerateAuthoringComponent]
    public struct Targetable : IComponentData
    {
        public TargetType TargetType;
        public int ID;
    }

    public enum TargetType {
        Angel, Daemon, Human // More Types of be added 

    }
}