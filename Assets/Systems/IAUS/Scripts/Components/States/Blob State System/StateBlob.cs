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
    public struct StateAsset
    {
        public Identity ID;
        public ConsiderationScoringData Health;
        public ConsiderationScoringData Distance;
        public ConsiderationScoringData Timer;
        public ConsiderationScoringData ManaAmmo;
        public ConsiderationScoringData TargetInRange;

       

    }
    public struct AIStateBlobAsset
    {
        public BlobArray<StateAsset> Array;

        public int GetConsiderationIndex(Identity identify)
        {
            int index = -1;
            for (int i = 0; i < Array.Length; i++)
            {
                if (Array[i].ID.Equals(identify))
                {
                    index = i;
                    return index;
                }
            }
            return index;
        }
    }
    public struct Identity
    {
        public NPCLevel NPCLevel;
        public int FactionID;
        public AIStates aIStates;
        public Difficulty Difficulty;

        public override string ToString()
        {
            return NPCLevel.ToString() + " " + FactionID.ToString() + " " + Difficulty.ToString() + " "+ aIStates.ToString();
        }
    }

    [UpdateBefore(typeof(IAUS.ECS.Systems.IAUSBrainSetupSystem))]
    public class SetupAIStateBlob : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((EntityQueryBuilder.F_DDD<Patrol, IAUSBrain, SetupBrainTag>)((ref Patrol p, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                p.stateRef = CreateReference();
                p.Index = p.stateRef.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    aIStates = AIStates.Patrol,
                    FactionID = brain.factionID,
                    //TODO fill out all Levels
                    NPCLevel = brain.NPCLevel
                });
            }));
            Entities.ForEach((EntityQueryBuilder.F_DDD<Traverse, IAUSBrain, SetupBrainTag>)((ref Traverse p, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                p.stateRef = CreateReference();
                p.Index = p.stateRef.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    aIStates = AIStates.Traverse,
                    FactionID = brain.factionID,
                    NPCLevel = brain.NPCLevel
                });
            }));

            Entities.ForEach((EntityQueryBuilder.F_DDD<Wait, IAUSBrain, SetupBrainTag>)((ref Wait w, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                w.stateRef = CreateReference();
                w.Index = w.stateRef.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    aIStates = AIStates.Wait,
                    FactionID = brain.factionID,
                    NPCLevel = brain.NPCLevel
                });
            }));
            Entities.ForEach((EntityQueryBuilder.F_DDD<GatherResourcesState, IAUSBrain, SetupBrainTag>)((ref GatherResourcesState G, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                G.stateRef = CreateReference();
                G.Index = G.stateRef.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    aIStates = G.name,
                    FactionID = brain.factionID,
                    NPCLevel = brain.NPCLevel
                }); ;
            }));

            Entities.ForEach((EntityQueryBuilder.F_DDD<RepairState, IAUSBrain, SetupBrainTag>)((ref RepairState G, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                G.stateRef = CreateReference();
                G.Index = G.stateRef.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    aIStates = G.name,
                    FactionID = brain.factionID,
                    NPCLevel = brain.NPCLevel
                }); ;
            }));

            Entities.ForEach((EntityQueryBuilder.F_DDD<SpawnDefendersState, IAUSBrain, SetupBrainTag>)((ref SpawnDefendersState G, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                G.stateRef = CreateReference();
                G.Index = G.stateRef.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    aIStates = G.name,
                    FactionID = brain.factionID,
                    NPCLevel = brain.NPCLevel
                }); ;
            }));

            Entities.ForEach((DynamicBuffer<AttackTypeInfo> attacks, ref IAUSBrain brain, ref SetupBrainTag tag, ref AttackTargetState a) => {
                for (int i = 0; i < attacks.Length; i++)
                {
                    AttackTypeInfo attack = attacks[i];
                    attack.stateRef = CreateReference();

                    attack.Index = attack.stateRef.Value.GetConsiderationIndex(new Identity()
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
                        FactionID = 1,
                        NPCLevel = brain.NPCLevel
                    });

                    attacks[i] = attack;
                }
                a.HighScoreAttack = attacks[0];

            });

        }

        BlobAssetReference<AIStateBlobAsset> CreateReference()
        {
            using var blobBuilder = new BlobBuilder(Allocator.Temp);
            ref var stateBlobAsset = ref blobBuilder.ConstructRoot<AIStateBlobAsset>();
            var assign = StateTextFileReader.SetupStateAsset();

            var array = blobBuilder.Allocate(ref stateBlobAsset.Array, assign.Length);

            for (int i = 0; i < assign.Length; i++)
            {
                array[i] = assign[i];
            }


            BlobAssetReference<AIStateBlobAsset> reference = blobBuilder.CreateBlobAssetReference<AIStateBlobAsset>(Allocator.Persistent);

            return reference;
        }

    }
   
}