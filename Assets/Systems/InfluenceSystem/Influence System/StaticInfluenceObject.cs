using Unity.Entities;

namespace DreamersInc.InflunceMapSystem
{
    public struct StaticInfluenceObject : IComponentData
    {
        public float ProtectionLevel;
        public bool Destructible;
    }

   
}