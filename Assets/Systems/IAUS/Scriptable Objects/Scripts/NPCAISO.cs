using System.Collections.Generic;
using IAUS.ECS.Component;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
using UnityEngine;


namespace IAUS.ECS
{
    [CreateAssetMenu( fileName = "NPC AI states", menuName = "IAUS/NPC AI", order = 51)]
    public class NPCAISO : ScriptableObject
    {
        public string Name;
        public Difficulty Difficulty;
        public NPCLevel NpcLevel;
        public int FactionID;
        public List<State> States;
        
    }

    [System.Serializable]
    public class State
    {
        public string Name;
        public List<ConsiderationForSO> Considerations;
    }
    [System.Serializable]
    public class ConsiderationForSO
    {
        public Considerations Consideration;
        public ConsiderationScoringData Scoring;
    }
}
