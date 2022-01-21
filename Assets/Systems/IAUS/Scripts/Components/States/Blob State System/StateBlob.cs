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
    public struct Identity
    {
        public NPCLevel NPCLevel;
        public int FactionID; 
        public AIStates aIStates;
        public Difficulty Difficulty;
    }

    public struct StateAsset
    {
        public Identify Identify;
        public ConsiderationScoringData Health;
        public ConsiderationScoringData Distance;
        public ConsiderationScoringData Timer;
        public ConsiderationScoringData ManaAmmo;
        public ConsiderationScoringData TargetInRange;



    }
    public struct AIStateBlobAsset {
        public BlobArray<StateAsset> Array;

        public int GetConsiderationIndex(Identify identify)
        {
            int index = -1;
            for (int i = 0; i < Array.Length; i++)
            {
                if (Array[i].Identify.Equals(identify))
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
        public int FactionID;
        public AIStates aIStates;
        public Difficulty Difficulty;
    }

    [UpdateBefore(typeof(IAUS.ECS.Systems.IAUSBrainSetupSystem))]
    public class SetupAIStateBlob : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref Patrol p, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                p.stateRef = CreateReference();
                p.Index = p.stateRef.Value.GetConsiderationIndex(new Identify() {
                    Difficulty = Difficulty.Normal,
                    aIStates = AIStates.Patrol,
                    FactionID = brain.factionID,
                    //TODO fill out all Levels
                    NPCLevel = NPCLevel.Grunt
                });
            });
            Entities.ForEach((ref Traverse p, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                p.stateRef = CreateReference();
                p.Index = p.stateRef.Value.GetConsiderationIndex(new Identify()
                {
                    Difficulty = Difficulty.Normal,
                    aIStates = AIStates.Traverse,
                    FactionID = brain.factionID,
                    NPCLevel = NPCLevel.Grunt
                });
            });

            Entities.ForEach((ref Wait w, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                w.stateRef = CreateReference();
                w.Index = w.stateRef.Value.GetConsiderationIndex(new Identify()
                {
                    Difficulty = Difficulty.Normal,
                    aIStates = AIStates.Wait,
                    FactionID = brain.factionID,
                    NPCLevel = NPCLevel.Grunt
                });
            });
            Entities.ForEach((ref GatherResourcesState G, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                G.stateRef = CreateReference();
                G.Index = G.stateRef.Value.GetConsiderationIndex(new Identify()
                {
                    Difficulty = Difficulty.Normal,
                    aIStates = AIStates.GatherResources,
                    FactionID = brain.factionID,
                    NPCLevel = brain.NPCLevel
                }); ;
            });

            Entities.ForEach((DynamicBuffer<AttackTypeInfo> attacks, ref IAUSBrain brain, ref SetupBrainTag tag, ref AttackTargetState a ) => {
                for (int i = 0; i < attacks.Length; i++)
                {
                    AttackTypeInfo attack = attacks[i];

                    attack.stateRef = CreateReference();

                    attack.Index = attack.stateRef.Value.GetConsiderationIndex(new Identify()
                    {
                        Difficulty = Difficulty.Normal,
                        aIStates = attack.style switch
                        {
                            AttackStyle.Melee => AIStates.AttackMelee,
                            AttackStyle.MagicMelee => AIStates.AttackMelee, //TODO Make Magic Attack AI State,
                            AttackStyle.Range => AIStates.AttackRange,
                            AttackStyle.MagicRange => AIStates.AttackRange,//TODO Make Magic Attack Range AI State,
                            _ => throw new ArgumentOutOfRangeException(nameof(attack.style), $"Not expected direction value: {attack.style}"),
                        },
                        FactionID = brain.factionID,
                        NPCLevel = brain.NPCLevel
                    });
                    attacks[i] = attack;
                }
            });

        }

        BlobAssetReference<AIStateBlobAsset> CreateReference() {
            using var blobBuilder = new BlobBuilder(Allocator.Temp);
            ref var stateBlobAsset = ref blobBuilder.ConstructRoot<AIStateBlobAsset>();
            var assign = Reader.FileRead();

            var array = blobBuilder.Allocate(ref stateBlobAsset.Array, assign.Length);

            for (int i = 0; i < assign.Length; i++)
            {
                array[i] = assign[i];
            }


            BlobAssetReference <AIStateBlobAsset> reference = blobBuilder.CreateBlobAssetReference<AIStateBlobAsset>(Allocator.Persistent);

            return reference;
        }

    }
    public class Reader
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
                    Identify = new Identify()
                    {
                        Difficulty = (Difficulty)Enum.Parse(typeof(Difficulty), parts[0]),
                        NPCLevel = (NPCLevel)Enum.Parse(typeof(NPCLevel), parts[1]),
                        FactionID =int.TryParse(parts[2], out int result) ? result:0,
                        aIStates = (AIStates)Enum.Parse(typeof(AIStates), parts[3])
                    },
                    Health = LineRead(4, lines[i]),
                    Distance = LineRead(11, lines[i]),
                    Timer = LineRead(18, lines[i]),
                    ManaAmmo = LineRead(25,lines[i]),
                    TargetInRange = LineRead(32,lines[i])
                };
              
            }
            return array;
        }

        static ConsiderationScoringData LineRead(int StartPoint, string Line)
        {
            ConsiderationScoringData output = new ConsiderationScoringData() {
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
}