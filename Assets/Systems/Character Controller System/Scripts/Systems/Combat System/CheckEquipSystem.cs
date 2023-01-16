using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DreamersInc.ComboSystem;
using Dreamers.InventorySystem;
using Dreamers.InventorySystem.Interfaces;

namespace MotionSystem
{
    public partial class CheckEquipSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((CharacterInventory inventory, PlayerComboComponent combo, Command command) =>
            {
                command.WeaponIsEquipped = combo.WeaponEquipped = inventory.Equipment.EquippedWeapons.TryGetValue(WeaponSlot.Primary, out _);


            }).Run();
        }
    }
}
