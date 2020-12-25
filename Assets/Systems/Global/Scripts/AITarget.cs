using Unity.Entities;

namespace Global.Component
{
    [System.Serializable]
    [GenerateAuthoringComponent]
    public struct AITarget : IComponentData
    {
        public TargetType Type;
        //public float3 Position; // This need to be removed 
        // Reference Local To World Instead

    }

    public enum TargetType
    {
        None, Character, Location, Vehicle
    }
  

}