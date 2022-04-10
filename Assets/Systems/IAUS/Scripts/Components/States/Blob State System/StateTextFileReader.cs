using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using DreamersInc.InflunceMapSystem;
using System;
using IAUS.ECS.Component;
using IAUS.ECS.Consideration;
namespace IAUS.ECS.StateBlobSystem
{
    public class StateTextFileReader
    {

        public static StateAsset[] FileRead()
        {
            TextAsset textFile = Resources.Load("StateTest") as TextAsset;
            var lines = textFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            StateAsset[] array = new StateAsset[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                array[i] = new StateAsset()
                {
                    ID = new Identity()
                    {
                        Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), parts[0]),
                        NPCLevel = (NPCLevel)Enum.Parse(typeof(NPCLevel), parts[1]),
                        FactionID = int.TryParse(parts[2], out int result) ? result : 0,
                        aIStates = (AIStates)Enum.Parse(typeof(AIStates), parts[3])
                    },
                    Health = LineRead(4, lines[i]),
                    Distance = LineRead(11, lines[i]),
                    Timer = LineRead(18, lines[i]),
                    ManaAmmo = LineRead(25, lines[i]),
                    TargetInRange = LineRead(32, lines[i])
                };

            }

            return array;

        }
        public static StateAsset[] SetupStateAsset()
        {
            TextAsset textFile = Resources.Load("Creature List") as TextAsset;
            var lines = textFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            List<StateAsset> array = new List<StateAsset>();
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                var stateParts = parts[3].Split(';');
                foreach (var state in stateParts)
                {
                    array.Add( new StateAsset()
                    {
                        ID = new Identity()
                        {
                            Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), parts[0]),
                            NPCLevel = (NPCLevel)Enum.Parse(typeof(NPCLevel), parts[1]),
                            FactionID = int.TryParse(parts[2], out int result) ? result : 0,
                            aIStates =(AIStates)Enum.Parse(typeof(AIStates), state)
                        }
                    });
                }
            }
            SetupConsideration(array, "Consideration files/Health", Considerations.Health);
            SetupConsideration(array, "Consideration files/DistanceToTarget", Considerations.DistanceToTarget);
            SetupConsideration(array, "Consideration files/Distance to place of Interest", Considerations.DistanceToPOI);
            SetupConsideration(array, "Consideration files/ManaAmmo", Considerations.ManaAmmo);
            SetupConsideration(array, "Consideration files/Time", Considerations.Time);

#if UNITY_EDITOR
            Debug.Log(array.Count);
#endif

            return array.ToArray();
        }
        public static void SetupConsideration(List<StateAsset> array , string textFilePath, Considerations consideration)
        {
            TextAsset textFile = Resources.Load(textFilePath) as TextAsset;
            var lines = textFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                var tempID = new Identity()
                {
                    Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), parts[0]),
                    NPCLevel = (NPCLevel)Enum.Parse(typeof(NPCLevel), parts[1]),
                    FactionID = int.TryParse(parts[2], out int result) ? result : 0,
                    aIStates = (AIStates)Enum.Parse(typeof(AIStates), parts[3])
                };
                int index = GetIndexOfIndentity(tempID, array);

                if (index == -1)
                    Debug.Log(tempID.ToString());
                else
                {
                    var temp = array[index];
                    switch (consideration)
                    {
                        case Considerations.Health:
                            temp.Health = LineRead(4, lines[i]);
                            break;
                        case Considerations.DistanceToTarget:
                            temp.TargetInRange = LineRead(4, lines[i]);
                            break;
                        case Considerations.DistanceToPOI:
                            temp.Distance = LineRead(4, lines[i]);
                            break;
                        case Considerations.Time:
                            temp.Timer = LineRead(4, lines[i]);
                            break;
                        case Considerations.ManaAmmo:
                            temp.ManaAmmo = LineRead(4, lines[i]);
                            break;

                    }
                    array[index] = temp;
                }
            }
        }

        public static int GetIndexOfIndentity(Identity ID, List<StateAsset> StateArray)
        {
            int index = -1;
            for (int i = 0; i < StateArray.Count; i++)
            {
                if (StateArray[i].ID.Equals(ID))
                {
                    index = i;
                    return index;
                }
            }
            return index;

        }


        static ConsiderationScoringData LineRead(int StartPoint, string Line)
        {
            ConsiderationScoringData output = new ConsiderationScoringData()
            {
                //  responseType = ResponseType.none
            };

            var parts = Line.Split(',');

            if (bool.Parse(parts[StartPoint]))
            {

                output = new ConsiderationScoringData()
                {
                    Inverse = bool.TryParse(parts[StartPoint + 1], out var b) ? b : false,
                    responseType = (ResponseType)Enum.Parse(typeof(ResponseType), parts[StartPoint + 2]),
                    M = float.TryParse(parts[StartPoint + 3], out var M) ? M : 0,
                    K = float.TryParse(parts[StartPoint + 4], out var K) ? K : 0,
                    B = float.TryParse(parts[StartPoint + 5], out var B) ? B : 0,
                    C = float.TryParse(parts[StartPoint + 6], out var C) ? C : 0
                };
            }
            return output;
        }
        
    }

    public enum Considerations { Health, DistanceToTarget, DistanceToPOI,Time, ManaAmmo}
}