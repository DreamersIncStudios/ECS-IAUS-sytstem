using DreamersIncStudio.FactionSystem;
using Unity.Entities;

namespace DreamersInc.InfluenceMapSystem
{

    [System.Serializable]
    public struct InfluenceComponent : IComponentData
    {
        public int InfluenceValue { get; private set; }

        public InfluenceComponent(FactionNames factionID,int influenceValue, float rangeOfInfluence)
        {
            InfluenceValue = influenceValue;
            this.FactionID = factionID;
            DetectionRadius = rangeOfInfluence;
        }

        public FactionNames FactionID;
        public float DetectionRadius { get; set; }
        
       
  
    }
}