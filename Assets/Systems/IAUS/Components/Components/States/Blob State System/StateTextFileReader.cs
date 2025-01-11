using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using IAUS.ECS.Consideration;
using Sirenix.Utilities;

namespace IAUS.ECS.StateBlobSystem
{
    public static class StateTextFileReader
    {
        public static StateAsset[] SetupStateAsset()
        {
            var npcStates = Resources.LoadAll<NPCAISO>(@"NPC States");
            return npcStates.IsNullOrEmpty() ? null : (from npcState in npcStates from state in npcState.States select CreateStateAsset(npcState, state)).ToArray();
        }

        // Local function to create a StateAsset
        private static StateAsset CreateStateAsset(NPCAISO npcState, State state)
        {
            var stateAsset = new StateAsset
            {
                ID = new Identity
                {
                    Difficulty = npcState.Difficulty,
                    NPCLevel = npcState.NPCLevel,
                    FactionID = npcState.FactionID,
                    AIStates = state.StateName
                }
            };
            foreach (var consideration in state.Considerations)
            {
                switch (consideration.ConsiderationType)
                {
                    case ConsiderationType.Health:
                        stateAsset.Health = consideration.Scoring;
                        break;
                    case ConsiderationType.DistanceToTargetEnemy:
                        stateAsset.DistanceToTargetEnemy = consideration.Scoring;
                        break;
                    case ConsiderationType.DistanceToTargetLocation:
                        stateAsset.DistanceToTargetLocation = consideration.Scoring;
                        break;
                    case ConsiderationType.DistanceToTargetAlly:
                        stateAsset.DistanceToTargetAlly = consideration.Scoring;
                        break;
                    case ConsiderationType.DistanceToPOI:
                        stateAsset.DistanceToPlaceOfInterest = consideration.Scoring;
                        break;
                    case ConsiderationType.Time:
                        stateAsset.Timer = consideration.Scoring;
                        break;
                    case ConsiderationType.ManaAmmo:
                        stateAsset.ManaAmmo = consideration.Scoring;
                        break;
                    case ConsiderationType.EnemyInfluence:
                        stateAsset.EnemyInfluence = consideration.Scoring;
                        break;
                    case ConsiderationType.FriendlyInfluence:
                        stateAsset.FriendlyInfluence = consideration.Scoring;
                        break;
                    case ConsiderationType.ManaAmmo2:
                        stateAsset.ManaAmmo2 = consideration.Scoring;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            return stateAsset;
        }


    }


    public enum ConsiderationType
    {
        Health,
        DistanceToTargetEnemy,
        DistanceToTargetLocation,
        DistanceToTargetAlly,
        DistanceToPOI,
        Time,
        ManaAmmo,
        EnemyInfluence,
        FriendlyInfluence,
        ManaAmmo2,
    }
}