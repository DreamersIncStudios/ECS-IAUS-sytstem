using Unity.Entities;
using UnityEngine;

namespace DreamersStudio.TargetingSystem
{
    [GenerateAuthoringComponent]
    public struct TargetBuffer : IBufferElementData
    {
        public Target target;

        public static implicit operator Target(TargetBuffer e) { return e; }
        public static implicit operator TargetBuffer(Target e) { return new TargetBuffer { target = e }; }
    }

    [System.Serializable]
    public struct Target {
        public bool isFriendly;
        public bool isVisible;
        public float CameraAngle;
        public int ID;

    }

}