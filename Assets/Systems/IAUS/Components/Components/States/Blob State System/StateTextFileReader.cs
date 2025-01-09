using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using DreamersInc.InflunceMapSystem;
using System;
using System.Linq;
using IAUS.ECS.Component;
using IAUS.ECS.Consideration;
using Sirenix.Utilities;

namespace IAUS.ECS.StateBlobSystem
{
    public class StateTextFileReader
    {

       public static StateAsset[] SetupStateAsset()
       {
    var npcStates = Resources.LoadAll<NPCAISO>(@"NPC States");
    if (npcStates.IsNullOrEmpty()) return null;

    var result = new List<StateAsset>();

    foreach (var npcState in npcStates)
    {
        foreach (var state in npcState.States)
        {
            result.Add(CreateStateAsset(npcState, state));
        }
    }

    return result.ToArray();

    // Local function to create a StateAsset
    StateAsset CreateStateAsset(NPCAISO npcState, State state)
    {
        var stateAsset = new StateAsset {
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
            MapConsiderationToAsset(stateAsset, consideration);
        }

        return stateAsset;
    }

    // Function to map a ConsiderationType to its value
    void MapConsiderationToAsset(StateAsset asset, ConsiderationForSO consideration)
    {
        switch (consideration.ConsiderationType)
        {
            case ConsiderationType.Health:
                asset.Health = consideration.Scoring;
                break;
            case ConsiderationType.DistanceToTargetEnemy:
                asset.DistanceToTargetEnemy = consideration.Scoring;
                break;
            case ConsiderationType.DistanceToTargetLocation:
                asset.DistanceToTargetLocation = consideration.Scoring;
                break;
            case ConsiderationType.DistanceToTargetAlly:
                asset.DistanceToTargetAlly = consideration.Scoring;
                break;
            case ConsiderationType.DistanceToPOI:
                asset.DistanceToPlaceOfInterest = consideration.Scoring;
                break;
            case ConsiderationType.Time:
                asset.Timer = consideration.Scoring;
                break;
            case ConsiderationType.ManaAmmo:
                asset.ManaAmmo = consideration.Scoring;
                break;
            case ConsiderationType.EnemyInfluence:
                asset.EnemyInfluence = consideration.Scoring;
                break;
            case ConsiderationType.FriendlyInfluence:
                asset.FriendlyInfluence = consideration.Scoring;
                break;
            case ConsiderationType.ManaAmmo2:
                asset.ManaAmmo2 = consideration.Scoring;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

        static ConsiderationScoringData LineRead( string line, int startPoint=4)
        {
            ConsiderationScoringData output = new();

            var parts = line.Split(',');

            if (bool.Parse(parts[startPoint]))
            {
                output = new ConsiderationScoringData()
                {
                    Inverse = bool.TryParse(parts[startPoint + 1], out var b) && b,
                    responseType = (ResponseType)Enum.Parse(typeof(ResponseType), parts[startPoint + 2]),
                    M = float.TryParse(parts[startPoint + 3], out var M) ? M : 0,
                    K = float.TryParse(parts[startPoint + 4], out var K) ? K : 0,
                    B = float.TryParse(parts[startPoint + 5], out var B) ? B : 0,
                    C = float.TryParse(parts[startPoint + 6], out var C) ? C : 0
                };
            }
            return output;
        }
        
    }

    public enum ConsiderationType { Health, DistanceToTargetEnemy,DistanceToTargetLocation,DistanceToTargetAlly, DistanceToPOI,Time, ManaAmmo, EnemyInfluence,FriendlyInfluence, ManaAmmo2, }
}