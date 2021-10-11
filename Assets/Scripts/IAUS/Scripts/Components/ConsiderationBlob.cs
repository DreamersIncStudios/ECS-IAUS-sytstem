using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using DreamersInc.InflunceMapSystem;
using System;
namespace IAUS.ECS.Consideration
{
    public struct ConsiderationBlobAsset
    {
        public BlobArray<ConsiderAsset> Array;

        public ConsiderationScoringData GetConsideration(Identify identify)
        {
            for (int i = 0; i < Array.Length; i++)
            {
                if (Array[i].Indentity.Equals(identify))
                    return Array[i].Data;
            }

            return new ConsiderationScoringData();
        }

    }

    public struct Identify
    {
        public NPCLevel NPCLevel;
        public Faction Faction;
        public AIStates aIStates;
        public int Level;

    }
    public enum NPCLevel
    {
        Grunt, Specialist
    }

    public struct ConsiderAsset
    {
        public Identify Indentity;
        public ConsiderationScoringData Data;
    }
    public class SetupConsiderationBlobAssetSystem : ComponentSystem
    {
        protected override void OnStartRunning()
        {
            using var blobBuilder = new BlobBuilder(Allocator.Temp);
            ref var healthBlobAsset = ref blobBuilder.ConstructRoot<ConsiderationBlobAsset>();
            ref var distanceBlobAsset = ref blobBuilder.ConstructRoot<ConsiderationBlobAsset>();
            TextAsset healthFile = Resources.Load<TextAsset>("Consideration/Health");
            TextAsset distanceFile = Resources.Load<TextAsset>("Consideration/Distance");
            var healthLines = healthFile.text.Split(new[] {Environment.NewLine},StringSplitOptions.RemoveEmptyEntries);
            var distanceLines = distanceFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);


            var healthArray = blobBuilder.Allocate(ref healthBlobAsset.Array, healthLines.Length);
            var distanceArray = blobBuilder.Allocate(ref distanceBlobAsset.Array, distanceLines.Length);

            for (int i = 0; i < healthLines.Length; i++)
            {
                healthArray[i] = ConvertToAsset(healthLines[i]);
            }
            for (int i = 0; i < distanceLines.Length; i++)
            {
                distanceArray[i] = ConvertToAsset(distanceLines[i]);
            }


        }

        protected override void OnUpdate()
        {

        }

        ConsiderAsset ConvertToAsset(string Line) {
           var parts = Line.Split(',');
            return new ConsiderAsset {
                Indentity = new Identify()
                {
                    NPCLevel = (NPCLevel)Enum.Parse(typeof(NPCLevel), parts[0]),
                    Faction = (Faction)Enum.Parse(typeof(Faction), parts[1]),
                    aIStates = (AIStates)Enum.Parse(typeof (AIStates),parts[2]),
                    Level = int.TryParse(parts[3], out var p) ? p : 0
                },
                Data = new ConsiderationScoringData() {
                    Inverse = bool.TryParse(parts[4], out var b) ? b : false,
                    responseType =(ResponseType)Enum.Parse(typeof(ResponseType),parts[5]),
                    M= int.TryParse(parts[6], out var M) ? M : 0,
                    K= int.TryParse(parts[7], out var K) ? K : 0,
                    B= int.TryParse(parts[8], out var B) ? B : 0,
                    C= int.TryParse(parts[9], out var C) ? C : 0
                }
                
            };

        }

    }
}