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

        public ConsiderationScoringData GetConsideration(Identify identify)
        {
            for (int i = 0; i < Array.Length; i++)
            {
                if (Array[i].Indentity.Equals(identify))
                {
                    return Array[i].Data;
                    Debug.Log("tre");
                }
            }

            return new ConsiderationScoringData();
        }

    }

    public struct Identify
    {
        public NPCLevel NPCLevel;
        public Faction Faction;
        public AIStates aIStates;
        public Difficulty Difficulty;
    }

    public enum NPCLevel
    {
        Grunt, Specialist
    }
    public enum Difficulty { Easy, Normal, Hard}

    public struct ConsiderAsset
    {
        public Identify Indentity;
        public ConsiderationScoringData Data;
    }
    public class SetupConsiderationBlobAssetSystem : ComponentSystem
    {
        BlobAssetReference<ConsiderationBlobAsset> copyOfHealth;
        BlobAssetReference<ConsiderationBlobAsset> copyOfDistance;

        protected override void OnStartRunning()
        {
            using var healthBuilder = new BlobBuilder(Allocator.Temp);
            using var distanceBuilder = new BlobBuilder(Allocator.Temp);
            ref var healthBlobAsset = ref healthBuilder.ConstructRoot<ConsiderationBlobAsset>();
            ref var distanceBlobAsset = ref distanceBuilder.ConstructRoot<ConsiderationBlobAsset>();
            TextAsset healthFile = Resources.Load("Considerations/Health") as TextAsset;
            TextAsset distanceFile = Resources.Load<TextAsset>("Considerations/Distance");
            var healthLines = healthFile.text.Split(new[] {Environment.NewLine},StringSplitOptions.RemoveEmptyEntries);
            var distanceLines = distanceFile.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);


            var healthArray = healthBuilder.Allocate(ref healthBlobAsset.Array, healthLines.Length);
            var distanceArray = distanceBuilder.Allocate(ref distanceBlobAsset.Array, distanceLines.Length);

            for (int i = 0; i < healthLines.Length; i++)
            {
                healthArray[i] = ConvertToAsset(healthLines[i]);
            }
            for (int i = 0; i < distanceLines.Length; i++)
            {
                distanceArray[i] = ConvertToAsset(distanceLines[i]);
            }
            copyOfHealth = healthBuilder.CreateBlobAssetReference<ConsiderationBlobAsset>(Allocator.Persistent);
            copyOfDistance = distanceBuilder.CreateBlobAssetReference<ConsiderationBlobAsset>(Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
            Entities.ForEach((ref Patrol p, ref IAUSBrain brain, ref SetupBrainTag Tag) => {

                p.HealthRatio = copyOfHealth.Value. GetConsideration(new Identify(
                    )
                {
                    aIStates = AIStates.Patrol,
                    Difficulty = Difficulty.Normal,
                    Faction = Faction.Enemy,
                    NPCLevel = NPCLevel.Grunt
                });
                p.DistanceToPoint= copyOfDistance.Value.GetConsideration(new Identify(
                         )
                {
                    aIStates = AIStates.Patrol,
                    Difficulty = Difficulty.Normal,
                    Faction = Faction.Enemy,
                    NPCLevel = NPCLevel.Grunt
                });

            });

            Entities.ForEach((ref Wait w, ref IAUSBrain brain, ref SetupBrainTag Tag) => {
                w.HealthRatio = copyOfHealth.Value.GetConsideration(new Identify(
                    )
                {
                    aIStates = AIStates.Patrol,
                    Difficulty = Difficulty.Normal,
                    Faction = Faction.Enemy,
                    NPCLevel = NPCLevel.Grunt
                });

            });

        }

        ConsiderAsset ConvertToAsset(string Line) {
           var parts = Line.Split(',');
            return new ConsiderAsset {
                Indentity = new Identify()
                {
                    NPCLevel = (NPCLevel)Enum.Parse(typeof(NPCLevel), parts[0]),
                    Faction = (Faction)Enum.Parse(typeof(Faction), parts[1]),
                    aIStates = (AIStates)Enum.Parse(typeof (AIStates),parts[2]),
                    Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty),parts[3])
                },
                Data = new ConsiderationScoringData() {
                    Inverse = bool.TryParse(parts[4], out var b) ? b : false,
                    responseType =(ResponseType)Enum.Parse(typeof(ResponseType),parts[5]),
                    M= float.TryParse(parts[6], out var M) ? M : 0,
                    K= float.TryParse(parts[7], out var K) ? K : 0,
                    B= float.TryParse(parts[8], out var B) ? B : 0,
                    C= float.TryParse(parts[9], out var C) ? C : 0
                }
                
            };

        }

    }
}