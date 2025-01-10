using System.Collections.Generic;
using IAUS.ECS.Component;
using IAUS.ECS.Consideration;
using IAUS.ECS.StateBlobSystem;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;


namespace IAUS.ECS
{
    [CreateAssetMenu( fileName = "NPC AI states", menuName = "IAUS/NPC AI", order = 51)]
    public class NPCAISO : ScriptableObject
    {
        public string Name { get => nameNPC; }
        public NPCLevel NPCLevel { get=> NpcLevel; }

        [FormerlySerializedAs("name")] [SerializeField] string nameNPC;
        public Difficulty Difficulty;
        public NPCLevel NpcLevel;
        public int FactionID;
        public List<State> States;
        
        public void OnValidate()
        {
            if (States.IsNullOrEmpty())
                return;

            // Use a HashSet to track unique states
            var uniqueStates = new HashSet<AIStates>();
            for (var i = 0; i < States.Count; i++)
            {
                // If the state already exists, it's a duplicate
                if (uniqueStates.Add(States[i].StateName) || States[i].StateName== AIStates.None) continue;
                Debug.LogWarning($"Duplicate AIState detected: {States[i].StateName}. Removing from the list.");
                States.RemoveAt(i);
                i--; // Adjust the index after removal
            }
        }
    }

    [System.Serializable]
    public class State
    {
        public AIStates StateName;
        public List<ConsiderationForSO> Considerations;
    }
    [System.Serializable]
    public class ConsiderationForSO
    {
        [FormerlySerializedAs("Consideration")] public ConsiderationType ConsiderationType;
        public ConsiderationScoringData Scoring;
    }
}
