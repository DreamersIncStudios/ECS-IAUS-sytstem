using Unity.Collections;
using Unity.Entities;
using IAUS.ECS.Component;
using IAUS.ECS.Consideration;
namespace IAUS.ECS.StateBlobSystem
{
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
        BlobAssetReference<AIStateBlobAsset> reference;
        protected override void OnCreate()
        {
            base.OnCreate();
            reference = CreateReference();
        }

        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((ref IAUSBrain brain, ref SetupBrainTag tag) =>
            {
                brain.State = reference;
                
            }).Run();
            Entities.WithoutBurst().ForEach((ref Patrol p, ref IAUSBrain brain, ref SetupBrainTag tag) => {
    
                p.SetIndex( reference.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = p.Name,
                    FactionID = brain.FactionID,
                    //TODO fill out all Levels
                    NPCLevel = brain.NPCLevel
                }));
            }).Run();
            Entities.WithoutBurst().ForEach((ref Traverse p, ref IAUSBrain brain, ref SetupBrainTag tag) => {
   
                p.SetIndex( reference.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = p.Name,
                    FactionID = brain.FactionID,
                    NPCLevel = brain.NPCLevel
                }));
            }).Run();

            Entities.WithoutBurst().ForEach((ref WanderQuadrant p, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                p.SetIndex( reference.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = p.Name,
                    FactionID = brain.FactionID,
                    NPCLevel = brain.NPCLevel
                }));
            }).Run();

            Entities.WithoutBurst().ForEach((ref Wait w, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                w.stateRef = reference;
                w.Index = w.stateRef.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = w.name,
                    FactionID = brain.FactionID,
                    NPCLevel = brain.NPCLevel
                });
            }).Run();
            Entities.WithoutBurst().ForEach((ref GatherResourcesState g, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                
                g.Index = reference.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = g.name,
                    FactionID = brain.FactionID,
                    NPCLevel = brain.NPCLevel
                }); ;
            }).Run();

            Entities.WithoutBurst().ForEach((ref RepairState G, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                G.Index = reference.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = G.name,
                    FactionID = brain.FactionID,
                    NPCLevel = brain.NPCLevel
                });             }).Run();

            //Entities.WithoutBurst().ForEach((ref SpawnDefendersState G, ref IAUSBrain brain, ref SetupBrainTag tag) => {
            //    reference = reference;
            //    G.Index = reference.Value.GetConsiderationIndex(new Identity()
            //    {
            //        Difficulty = Difficulty.Normal,
            //        aIStates = G.name,
            //        FactionID = brain.factionID,
            //        NPCLevel = brain.NPCLevel
            //    }); 
           // }).Run();
            Entities.WithoutBurst().ForEach((ref RetreatCitizen G, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                G.Index = reference.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = G.name,
                    FactionID = brain.FactionID,
                    NPCLevel = brain.NPCLevel
                }); ;
            }).Run();

            Entities.WithoutBurst().ForEach((ref TerrorizeAreaState G, ref IAUSBrain brain, ref SetupBrainTag tag) => {
                G.Index = reference.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = G.name,
                    FactionID = brain.FactionID,
                    NPCLevel = brain.NPCLevel
                });          }).Run();

            Entities.WithoutBurst().ForEach((ref IAUSBrain brain, ref SetupBrainTag tag, ref AttackState G) =>
            {
                G.SetIndex(reference.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = G.Name,
                    FactionID = brain.FactionID,
                    NPCLevel = brain.NPCLevel
                })); 

            }).Run();
            Entities.WithoutBurst().ForEach((ref IAUSBrain brain, ref SetupBrainTag tag, ref MeleeAttackSubState G) =>
            {
                G.SetIndex(reference.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = G.Name,
                    FactionID = brain.FactionID,
                    NPCLevel = brain.NPCLevel
                }));

            }).Run();
            Entities.WithoutBurst().ForEach((ref IAUSBrain brain, ref SetupBrainTag tag, ref EscapeThreat G) =>
            {
                G.SetIndex(reference.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = G.Name,
                    FactionID = brain.FactionID,
                    NPCLevel = brain.NPCLevel
                }));

            }).Run();

            Entities.WithoutBurst().ForEach((ref IAUSBrain brain, ref SetupBrainTag tag, ref MagicAttackSubState G) =>
            {
                G.SetIndex(reference.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = MagicAttackSubState.Name,
                    FactionID = brain.FactionID,
                    NPCLevel = brain.NPCLevel
                }));

            }).Run(); Entities.WithoutBurst().ForEach((ref IAUSBrain brain, ref SetupBrainTag tag, ref WeaponSkillsAttackSubState G) =>
            {
                G.SetIndex(reference.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = WeaponSkillsAttackSubState.Name,
                    FactionID = brain.FactionID,
                    NPCLevel = brain.NPCLevel
                }));

            }).Run(); Entities.WithoutBurst().ForEach((ref IAUSBrain brain, ref SetupBrainTag tag, ref RangedAttackSubState G) =>
            {
                G.SetIndex(reference.Value.GetConsiderationIndex(new Identity()
                {
                    Difficulty = Difficulty.Normal,
                    AIStates = RangedAttackSubState.Name,
                    FactionID = brain.FactionID,
                    NPCLevel = brain.NPCLevel
                }));

            }).Run();

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


            var blobAssetReference = blobBuilder.CreateBlobAssetReference<AIStateBlobAsset>(Allocator.Persistent);

            return blobAssetReference;
        }

    }

}