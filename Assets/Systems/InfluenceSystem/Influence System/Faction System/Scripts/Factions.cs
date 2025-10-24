using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DreamersIncStudio.FactionSystem
{
    [InternalBufferCapacity(0)]
    public struct Factions : IBufferElementData
    {
        public FactionNames Faction;
        public FixedList512Bytes<Relationship> Relationships; // < 512 bytes>

        public Factions(FactionData factionData)
        {
            Faction = factionData.Faction;
            Relationships = new FixedList512Bytes<Relationship>();
            foreach(var relationship in factionData.Relationships)
                Relationships.Add(relationship);
        }
        
    }
    
    [System.Serializable]
    public struct Relationship
    {
        public FactionNames Faction; // 4bytes
        [Range(-100,100)]public int Affinity; // 4bytes 
    }
    public enum Affinity { Hate,Negative, Neutral, Positive, Love }
    [System.Serializable]
    public struct FactionData
    {
        public FactionNames Faction;
        public List<Relationship> Relationships; // < 512 bytes>  
    }
}