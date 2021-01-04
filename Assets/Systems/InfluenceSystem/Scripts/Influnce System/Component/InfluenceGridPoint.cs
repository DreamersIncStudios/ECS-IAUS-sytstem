using Unity.Mathematics;
using Unity.Entities;

namespace InfluenceSystem.Component
{

    public struct InfluenceGridPoint : IComponentData
    {
        public int GridmapID;
        public GridValues Enemies; // This is be broken out based on NPC Faction later;
        public GridValues PlayerParty;

    } 

    [System.Serializable]
    public struct GridValues
    {
        public float Protection;
        public float Threat;

    }
}