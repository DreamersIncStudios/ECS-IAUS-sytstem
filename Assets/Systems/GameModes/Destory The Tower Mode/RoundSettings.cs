using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameModes.DestroyTheTower
{
    [System.Serializable]
    public class RoundSettings
    {
        public int Level; //Todo Write on Validate method to set level equal to +1 list index
        [Tooltip("Time limit in Mins")]
        public int TimeLimit;
        [Range(0, 50)]
        public int NumberOfTowers;
        public Rewards CompletionRewards;
    }
    [System.Serializable]
    public struct Rewards {
        public int ExpGained;
        public int GoldGained;
        public List< ScriptableObject> RewardItem;
    }

    public struct AdditionalWinConditions {
        public string Description;
        public List<Rewards> CompletionRewards;


    }
}