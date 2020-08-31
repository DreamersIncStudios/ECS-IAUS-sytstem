using UnityEngine;
using Unity.Entities;
using Dreamers.InventorySystem;
using Unity.Collections;
using Dreamers.InventorySystem.Base;

namespace IAUS.ECS2
{
    public struct InventoryConsiderationBuffer : IBufferElementData
    {
        public InventoryConsidederGeneralItem Consider;

        public static implicit operator InventoryConsidederGeneralItem(InventoryConsiderationBuffer e) { return e.Consider; }
        public static implicit operator InventoryConsiderationBuffer(InventoryConsidederGeneralItem e) { return new InventoryConsiderationBuffer { Consider = e }; }
    }

    public struct InventoryConsidederGeneralItem {
        public float Ratio { get {
                if (MaxConsider > 0)
                    return Mathf.Clamp01( count / MaxConsider);
                else 
                    return 0;
            } }
        public TypeOfGeneralItem ItemTypeToConsider;
        public int MaxConsider;
        public int count;
    
    }

    public class InventoryInspectionSystem : ComponentSystem
    {
        BufferFromEntity<InventoryConsiderationBuffer> BufferEntity ;

        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, CharacterInventory inventory) =>
            {
                 BufferEntity = GetBufferFromEntity<InventoryConsiderationBuffer>(false);

                DynamicBuffer<InventoryConsiderationBuffer> Buffer = BufferEntity[entity];
                for (int i = 0; i < Buffer.Length; i++)
                {
                    InventoryConsiderationBuffer Consider = Buffer[i];
                    {
                        Consider.Consider.count = 0;
                        foreach (ItemSlot item in inventory.Inventory.ItemsInInventory)
                        {
                            if (item.Item.Type == ItemType.General)
                            {
                                GeneralItemSO temp = (GeneralItemSO)item.Item;
                                if (temp.GeneralItemType == Consider.Consider.ItemTypeToConsider)
                                    Consider.Consider.count += item.Count;
                            }
                        }

                    }
                    Buffer[i] = Consider;
                }
              
            });
        }

    }


}
