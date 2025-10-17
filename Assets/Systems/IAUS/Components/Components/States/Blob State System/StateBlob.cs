using System;
using Unity.Collections;
using Unity.Entities;
using IAUS.ECS.Component;
using IAUS.ECS.Consideration;
using UnityEngine;

namespace IAUS.ECS.StateBlobSystem
{
    [Serializable]
    public struct StateAsset
    {
        public Identity ID;
        public ConsiderationScoringData Health;
        public ConsiderationScoringData DistanceToPlaceOfInterest;
        public ConsiderationScoringData Timer;
        public ConsiderationScoringData ManaAmmo;
        public ConsiderationScoringData ManaAmmo2;

        public ConsiderationScoringData DistanceToTargetLocation;
        public ConsiderationScoringData DistanceToTargetEnemy;
        public ConsiderationScoringData DistanceToTargetAlly;
        public ConsiderationScoringData EnemyInfluence;
        public ConsiderationScoringData FriendlyInfluence;
    }

     public struct AIStateBlobAsset
     {
         public BlobArray<StateAsset> Array;

         public int GetConsiderationIndex(Identity identify)
         {
             var index = -1;
             for (var i = 0; i < Array.Length; i++)
             {
                 if (!Array[i].ID.Equals(identify)) continue;
                 index = i;
                 return index;
             }
             return index;
         }
     }
     public struct Identity
     {
         public NPCLevel NPCLevel;
         public int FactionID;
         public AIStates AIStates;
         public Difficulty Difficulty;

         public override string ToString()
         {
             return NPCLevel.ToString() + " " + FactionID.ToString() + " " + Difficulty.ToString() + " " + AIStates.ToString();
         }
     }

     [UpdateBefore(typeof(IAUS.ECS.Systems.IAUSBrainSetupSystem))]
     public partial class SetupAIStateBlob : SystemBase
     {
         private BlobAssetReference<AIStateBlobAsset> reference;
         protected override void OnCreate()
         {
             base.OnCreate();
             reference = CreateReference();
         }

         //TODO Get diffultity from manager singleton 
         
         protected override void OnUpdate()
         {
     
             
             Entities.WithoutBurst().ForEach(( DynamicBuffer<StateData> statesToCheck, ref IAUSBrain brain, in SetupBrainTag tag) => {
                 brain.State = reference;
                 for (int i = 0; i < statesToCheck.Length; i++)
                 {
                     var s = statesToCheck[i];
                     Debug.Log(reference.Value.GetConsiderationIndex(new Identity
                     {
                         Difficulty = brain.Difficulty,
                         AIStates = s.State,
                         FactionID = (int)brain.FactionID,
                         NPCLevel = brain.NPCLevel
                     }));
                     s.SetIndex(reference.Value.GetConsiderationIndex(new Identity
                     {
                         Difficulty = brain.Difficulty,
                         AIStates = s.State,
                         FactionID = (int)brain.FactionID,
                         NPCLevel = brain.NPCLevel
                     }));
                     s.SetStatus(ActionStatus.Idle);
                     statesToCheck[i] = s; // write back
                 }
             }).Run();
       
           

       }

         BlobAssetReference<AIStateBlobAsset> CreateReference()
         {
             using var blobBuilder = new BlobBuilder(Allocator.Temp);
             ref var stateBlobAsset = ref blobBuilder.ConstructRoot<AIStateBlobAsset>();
             var assign = StateTextFileReader.SetupStateAsset();

             var array = blobBuilder.Allocate(ref stateBlobAsset.Array, assign.Length);

             for (var i = 0; i < assign.Length; i++)
             {
                 array[i] = assign[i];
             }


             var blobAssetReference = blobBuilder.CreateBlobAssetReference<AIStateBlobAsset>(Allocator.Persistent);

             return blobAssetReference;
         }

     }

 }