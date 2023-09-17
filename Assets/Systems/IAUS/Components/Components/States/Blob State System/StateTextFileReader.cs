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
namespace IAUS.ECS.StateBlobSystem
{
    public class StateTextFileReader
    {

       
        public static StateAsset[] SetupStateAsset()
        {
            var textFile = Resources.Load("Creature List") as TextAsset;
            if (textFile != null)
            {
                var lines = textFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                var array = (from t in lines
                    select t.Split(',')
                    into parts
                    let stateParts = parts[3].Split(';')
                    from state in stateParts
                    select new StateAsset()
                    {
                        ID = new Identity()
                        {
                            Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), parts[0]),
                            NPCLevel = (NPCLevel)Enum.Parse(typeof(NPCLevel), parts[1]),
                            FactionID = int.TryParse(parts[2], out var result) ? result : 0,
                            AIStates = (AIStates)Enum.Parse(typeof(AIStates), state)
                        }
                    }).ToList();
                SetupConsideration(array, "Consideration files/Health", Considerations.Health);
                SetupConsideration(array, "Consideration files/DistanceToTarget Enemy", Considerations.DistanceToTargetEnemy);
                SetupConsideration(array, "Consideration files/DistanceToTarget Ally", Considerations.DistanceToTargetAlly);
                SetupConsideration(array, "Consideration files/DistanceToTarget Location", Considerations.DistanceToTargetLocation);
                SetupConsideration(array, "Consideration files/Distance to place of Interest", Considerations.DistanceToPOI);
                SetupConsideration(array, "Consideration files/ManaAmmo", Considerations.ManaAmmo);
                SetupConsideration(array, "Consideration files/ManaAmmo2", Considerations.ManaAmmo2);
                SetupConsideration(array, "Consideration files/Time", Considerations.Time);
                SetupConsideration(array, "Consideration files/Enemy Influence", Considerations.EnemyInfluence);
                SetupConsideration(array, "Consideration files/Friendly Influence 1", Considerations.FriendlyInfluence);


                return array.ToArray();
            }
            else
                return null;
        }
        public static void SetupConsideration(List<StateAsset> array , string textFilePath, Considerations consideration)
        {
            TextAsset textFile = Resources.Load(textFilePath) as TextAsset;
            if (textFile == null) {
                throw new ArgumentOutOfRangeException(nameof(textFilePath), $"File not include in project/build " +
                    $"{textFilePath}");
            }
            var lines = textFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in lines)
            {
                var parts = t.Split(',');
                var tempID = new Identity()
                {
                    Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), parts[0]),
                    NPCLevel = (NPCLevel)Enum.Parse(typeof(NPCLevel), parts[1]),
                    FactionID = int.TryParse(parts[2], out int result) ? result : 0,
                    AIStates = (AIStates)Enum.Parse(typeof(AIStates), parts[3])
                };
                var index = GetIndexOfIdentity(tempID, array);

                if (index == -1)
                {
                   
#if UNITY_EDITOR
                    Debug.Log($"{tempID.NPCLevel} faction {tempID.FactionID} needs {tempID.AIStates} add to Creature List Text file");
#endif

                }

                else
                {
                    var temp = array[index];
                    switch (consideration)
                    {
                        case Considerations.Health:
                            temp.Health = LineRead(t);
                            break;
                        case Considerations.DistanceToTargetEnemy:
                            temp.DistanceToTargetEnemy = LineRead(t);
                            break;
                        case Considerations.DistanceToTargetAlly:
                            temp.DistanceToTargetAlly = LineRead(t);
                            break; 
                        case Considerations.DistanceToTargetLocation:
                            temp.DistanceToTargetLocation = LineRead(t);
                            break;
                        case Considerations.DistanceToPOI:
                            temp.DistanceToPlaceOfInterest = LineRead(t);
                            break;
                        case Considerations.Time:
                            temp.Timer = LineRead(t);
                            break;
                        case Considerations.ManaAmmo:
                            temp.ManaAmmo = LineRead(t);
                            break;
                        case Considerations.FriendlyInfluence:
                            temp.FriendlyInfluence = LineRead(t);
                            break;
                        case Considerations.EnemyInfluence:
                            temp.EnemyInfluence = LineRead(t);
                            break;

                        case Considerations.ManaAmmo2:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(consideration), consideration, null);
                    }
                    array[index] = temp;
                }
            }
        }

        private static int GetIndexOfIdentity(Identity ID, List<StateAsset> stateArray)
        {
            var index = -1;
            for (var i = 0; i < stateArray.Count; i++)
            {
                if (!stateArray[i].ID.Equals(ID)) continue;
                index = i;
                return index;
            }
            return index;

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

    public enum Considerations { Health, DistanceToTargetEnemy,DistanceToTargetLocation,DistanceToTargetAlly, DistanceToPOI,Time, ManaAmmo, EnemyInfluence,FriendlyInfluence, ManaAmmo2, }
}