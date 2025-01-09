using System;
using System.Collections.Generic;
using IAUS.ECS.Consideration;
using UnityEngine;

namespace IAUS.ECS
{
    [CreateAssetMenu(fileName = "NPC AI State Data", menuName = "IAUS/NPC AI State Data", order = 51)]
    public class NPCAIStateData : ScriptableObject
    {
        [SerializeField] string npcName;

        public string NPCName
        {
            get => npcName;
        }

        [SerializeField] private List<StateData> stateData;

        public List<StateData> StateData
        {
            get => stateData;
        }

        public void OnValidate()
        {
            if (stateData == null || stateData.Count == 0)
                return;

            // Use a HashSet to track unique states
            var uniqueStates = new HashSet<AIStates>();
            for (var i = 0; i < stateData.Count; i++)
            {
                // If the state already exists, it's a duplicate
                if (uniqueStates.Add(stateData[i].StateName) || stateData[i].StateName== AIStates.None) continue;
                Debug.LogWarning($"Duplicate AIState detected: {stateData[i].StateName}. Removing from the list.");
                stateData.RemoveAt(i);
                i--; // Adjust the index after removal
            }
        }

    }

    public class StateData
    {
        public AIStates StateName;
        public List<ConsiderationScoringData> Considerations;
    }
}