using Unity.Entities;

namespace DreamersInc.InfluenceMapSystem
{
    public struct StaticInfluenceObject : IComponentData
    {
        public float ProtectionLevel;
        public bool Destructible;
    }

   
}