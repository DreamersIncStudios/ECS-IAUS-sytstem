using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Entities;
namespace Dreamers.InventorySystem
{
    [GenerateAuthoringComponent]
    public struct Wallet : IComponentData
    {
 
        public uint Value;
    }

    public struct AddWalletValue : IComponentData {
        public uint Change;
    }

    public struct SubtractWalletValue : IComponentData
    {
        public uint Change;
    }
    public class WalletManagementSystem : SystemBase
    {
        private EntityQuery _walletHolderAdd;
        private EntityQuery _walletHolderSubtract;

        EntityCommandBufferSystem _entityCommandBufferSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _walletHolderAdd = GetEntityQuery(new EntityQueryDesc() { 
                All= new ComponentType[] { ComponentType.ReadOnly(typeof(AddWalletValue)), ComponentType.ReadWrite(typeof(Wallet))}
            });
            _walletHolderSubtract = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SubtractWalletValue)), ComponentType.ReadWrite(typeof(Wallet)) }
            });
        }

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new AddWalletValueJob()
            {
                WalletChunk = GetComponentTypeHandle<Wallet>(false),
                EntityChunk = GetEntityTypeHandle(),
                ChangeChunk = GetComponentTypeHandle<AddWalletValue>(true),
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter()

            }.ScheduleParallel(_walletHolderAdd, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);
            systemDeps = new SubtractWalletValueJob()
            {
                WalletChunk = GetComponentTypeHandle<Wallet>(false),
                EntityChunk = GetEntityTypeHandle(),
                ChangeChunk = GetComponentTypeHandle<SubtractWalletValue>(true),
                entityCommandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter()

            }.ScheduleParallel(_walletHolderSubtract, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;
        }

        public struct AddWalletValueJob : IJobChunk
        {
            public ComponentTypeHandle<Wallet> WalletChunk;
            [ReadOnly]public ComponentTypeHandle<AddWalletValue> ChangeChunk;
            [ReadOnly]public EntityTypeHandle EntityChunk;
            public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                NativeArray<Wallet> wallets = chunk.GetNativeArray(WalletChunk);
                NativeArray<AddWalletValue> changeValues = chunk.GetNativeArray(ChangeChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity entity = entities[i];
                    Wallet wallet = wallets[i];
                    AddWalletValue change = changeValues[i];
                    wallet.Value += change.Change;
                    //just a safety Check

                    if (wallet.Value >= 999999)
                    {
                        wallet.Value = 999999;
#if UNITY_EDITOR
                        Debug.LogWarning("Exceeding Max wallet Value. Value will not be added to wallet");
#endif
                    }
                    entityCommandBuffer.RemoveComponent<AddWalletValue>(chunkIndex, entity);

                    wallets[i] = wallet;
                }
            }
        }



        public struct SubtractWalletValueJob : IJobChunk
        {
            public ComponentTypeHandle<Wallet> WalletChunk;
            [ReadOnly] public ComponentTypeHandle<SubtractWalletValue> ChangeChunk;
            [ReadOnly] public EntityTypeHandle EntityChunk;
            public EntityCommandBuffer.ParallelWriter entityCommandBuffer;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
                NativeArray<Wallet> wallets = chunk.GetNativeArray(WalletChunk);
                NativeArray<SubtractWalletValue> changeValues = chunk.GetNativeArray(ChangeChunk);
                for (int i = 0; i < chunk.Count; i++)
                {
                    Entity entity = entities[i];
                    Wallet wallet = wallets[i];
                    SubtractWalletValue change = changeValues[i];
                    //just a safety Check

                    if (wallet.Value <= change.Change)
                    {
                        wallet.Value = 0;
#if UNITY_EDITOR
                        Debug.LogWarning("Value of Change greater than amount in wallet. Unable to take money from wallet. ");
#endif
                    }
                    else
                        wallet.Value -= change.Change;

                    entityCommandBuffer.RemoveComponent<SubtractWalletValue>(chunkIndex, entity);

                    wallets[i] = wallet;
                }
            }
        }
    }
    
}
