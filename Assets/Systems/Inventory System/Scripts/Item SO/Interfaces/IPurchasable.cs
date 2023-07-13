using UnityEngine;
using Dreamers.InventorySystem;

namespace Dreamers.InventorySystem.Interfaces
{
    public interface IPurchasable
    {
        [SerializeField]uint Value { get; }
        [SerializeField] uint MaxStackCount { get; }
        [SerializeField] bool Stackable { get; }

        //TODO COnsider adding can cell?
    }
}