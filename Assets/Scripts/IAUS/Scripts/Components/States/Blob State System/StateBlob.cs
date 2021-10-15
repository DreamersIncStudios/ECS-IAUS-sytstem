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
    public struct Identity {
        public NPCLevel NPCLevel;
        public Faction Faction;
        public AIStates aIStates;
        public Difficulty Difficulty;
    }

    public struct StateAsset {
        public Identify Identify;
        public ConsiderationScoringData Health;
        public ConsiderationScoringData Distance;
        public ConsiderationScoringData Timer;

    }

    public class Reader {

        public void FileRead() {
            TextAsset textFile = Resources.Load("StateTest") as TextAsset;
            var lines = textFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                var parts = lines[i].Split(',');
                ConsiderationScoringData TempHealth = new ConsiderationScoringData();
                if (bool.Parse(parts[4]))
                {

                    TempHealth = new ConsiderationScoringData()
                    {
                        Inverse = bool.TryParse(parts[4], out var b) ? b : false,
                        responseType = (ResponseType)Enum.Parse(typeof(ResponseType), parts[5]),
                        M = float.TryParse(parts[6], out var M) ? M : 0,
                        K = float.TryParse(parts[7], out var K) ? K : 0,
                        B = float.TryParse(parts[8], out var B) ? B : 0,
                        C = float.TryParse(parts[9], out var C) ? C : 0
                    };
                }

                new StateAsset()
                {
                    Identify = new Identify()
                    {
                        Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), parts[0]),
                        NPCLevel = (NPCLevel)Enum.Parse(typeof(NPCLevel), parts[1]),
                        Faction = (Faction)Enum.Parse(typeof(Faction), parts[2]),
                        aIStates = (AIStates)Enum.Parse(typeof(AIStates), parts[3])
                    },
                    Health = LineRead(4, lines[i]),
                    Distance = LineRead(11, lines[i]),
                    Timer = LineRead(18, lines[i]),
                };
            }
        }

        ConsiderationScoringData LineRead(int StartPoint, string Line) {
            ConsiderationScoringData output = new ConsiderationScoringData();
            {
                var parts = Line.Split(',');

                if (bool.Parse(parts[StartPoint]))
                {

                    output  = new ConsiderationScoringData()
                    {
                        Inverse = bool.TryParse(parts[StartPoint+1], out var b) ? b : false,
                        responseType = (ResponseType)Enum.Parse(typeof(ResponseType), parts[StartPoint+2]),
                        M = float.TryParse(parts[StartPoint+3 ], out var M) ? M : 0,
                        K = float.TryParse(parts[StartPoint+4], out var K) ? K : 0,
                        B = float.TryParse(parts[StartPoint+5], out var B) ? B : 0,
                        C = float.TryParse(parts[StartPoint+6], out var C) ? C : 0
                    };
                }


                return output;
        }
    
    }

}