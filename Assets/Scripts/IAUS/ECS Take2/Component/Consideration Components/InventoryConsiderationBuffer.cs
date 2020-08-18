using UnityEngine;
using Unity.Entities;
using Dreamers.InventorySystem;

namespace IAUS.ECS2
{
    public struct InventoryConsiderationBuffer : IBufferElementData
    {
        public InventoryConsidederGeneralItem Consider;

        public static implicit operator InventoryConsidederGeneralItem(InventoryConsiderationBuffer e) { return e.Consider; }
        public static implicit operator InventoryConsiderationBuffer(InventoryConsidederGeneralItem e) { return new InventoryConsiderationBuffer { Consider = e }; }
    }

    public struct InventoryConsidederGeneralItem {
        public float Ratio;
        public TypeOfGeneralItem ItemTypeToConsider;
    
    }
}
