using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using DreamersInc.InflunceMapSystem;
using System;
using IAUS.ECS.Component;

namespace IAUS.ECS.Consideration
{
    public struct ConsiderationBlobAsset
    {
        public BlobArray<ConsiderAsset> Array;

        public int GetConsiderationIndex(Identify identify)
        {
            int index = -1;
            for (int i = 0; i < Array.Length; i++)
            {
                if (Array[i].Indentity.Equals(identify))
                {
                    index = i;
                    return index;

                }
            }

            return index;
        }
    }

    public struct Identify
    {
        public NPCLevel NPCLevel;
        public Faction Faction;
        public AIStates aIStates;
        public Difficulty Difficulty;
    }

    public struct ConsiderAsset
    {
        public Identify Indentity;
        public ConsiderationScoringData Data;
    }

    [UpdateBefore(typeof(IAUS.ECS.Systems.IAUSBrainSetupSystem))]
    public class SetupConsiderationBlobAssetSystem : ComponentSystem
    {

        protected override void OnUpdate()
        {
            Entities.ForEach((ref Patrol p, ref IAUSBrain brain, ref SetupBrainTag Tag) =>
            {
                Debug.Log("");
                p.health = CreateReference("Considerations/Health", Allocator.Persistent);
                p.distance = CreateReference("Considerations/Distance", Allocator.Persistent);
                p.HrefIndex = p.health.Value.GetConsiderationIndex(new Identify()
                {
                    Difficulty = Difficulty.Normal,
                    aIStates = AIStates.Patrol,
                    Faction = Faction.Enemy,//brain.faction
                    NPCLevel = NPCLevel.Grunt

                });
                p.DrefIndex = p.distance.Value.GetConsiderationIndex(new Identify()
                {
                    Difficulty = Difficulty.Normal,
                    aIStates = AIStates.Patrol,
                    Faction = Faction.Enemy,//brain.faction
                    NPCLevel = NPCLevel.Grunt

                });
            });

            Entities.ForEach((ref Wait w, ref IAUSBrain brain, ref SetupBrainTag Tag) =>
            {
                w.health = CreateReference("Considerations/Health", Allocator.Persistent);
                w.timeLeft = CreateReference("Considerations/WaitTime", Allocator.Persistent);
                //   w.refIndex = 1;

            });

        }

        ConsiderAsset ConvertToAsset(string Line)
        {
            var parts = Line.Split(',');
            return new ConsiderAsset
            {
                Indentity = new Identify()
                {
                    NPCLevel = (NPCLevel)Enum.Parse(typeof(NPCLevel), parts[0]),
                    Faction = (Faction)Enum.Parse(typeof(Faction), parts[1]),
                    aIStates = (AIStates)Enum.Parse(typeof(AIStates), parts[2]),
                    Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), parts[3])
                },
                Data = new ConsiderationScoringData()
                {
                    Inverse = bool.TryParse(parts[4], out var b) ? b : false,
                    responseType = (ResponseType)Enum.Parse(typeof(ResponseType), parts[5]),
                    M = float.TryParse(parts[6], out var M) ? M : 0,
                    K = float.TryParse(parts[7], out var K) ? K : 0,
                    B = float.TryParse(parts[8], out var B) ? B : 0,
                    C = float.TryParse(parts[9], out var C) ? C : 0
                }

            };

        }

        BlobAssetReference<ConsiderationBlobAsset> CreateReference(string fileString, Allocator allocator)
        {

            using var blobBuilder = new BlobBuilder(Allocator.Temp);
            ref var considerBlobAsset = ref blobBuilder.ConstructRoot<ConsiderationBlobAsset>();
            TextAsset textFile = Resources.Load(fileString) as TextAsset;
            var lines = textFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);


            var arrray = blobBuilder.Allocate(ref considerBlobAsset.Array, lines.Length);

            for (int i = 0; i < lines.Length; i++)
            {
                arrray[i] = ConvertToAsset(lines[i]);
            }

            BlobAssetReference<ConsiderationBlobAsset> reference = blobBuilder.CreateBlobAssetReference<ConsiderationBlobAsset>(allocator);

            return reference;
        }

    }
}