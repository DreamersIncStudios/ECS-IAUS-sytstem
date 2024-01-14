using Dreamers.InventorySystem;
using Dreamers.InventorySystem.Interfaces;
using DreamersInc.ComboSystem;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace DreamersInc.CombatSystem
{
    [UpdateAfter(typeof(ComboInputSystem))]
    public partial class EquipSystem : SystemBase
    {

        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((CharacterInventory test, Command input) => {
                if (input.StateInfo.IsName("Grounded0"))
                {
                    if (test.Equipment.EquippedWeapons.TryGetValue(WeaponSlot.Primary, out _) && !input.WeaponIsEquipped)
                        input.WeaponIsEquipped = true;
                    if (!test.Equipment.EquippedWeapons.TryGetValue(WeaponSlot.Primary, out _) && input.WeaponIsEquipped)
                        input.WeaponIsEquipped = false;
                }
            }).Run();
            Entities.WithoutBurst().WithStructuralChanges().ForEach((Entity entity, Animator Anim, CharacterInventory character, ref DrawWeapon tag) => {

                character.Equipment.EquippedWeapons[WeaponSlot.Primary].DrawWeapon(Anim);
                EntityManager.RemoveComponent<DrawWeapon>(entity);
            }).Run();
            Entities.WithoutBurst().WithStructuralChanges().ForEach((Entity entity, Animator Anim, CharacterInventory character, ref StoreWeapon tag) =>
            {
                if (character.Equipment.EquippedWeapons.Count <= 0) return;
                if (character.Equipment.EquippedWeapons.TryGetValue(WeaponSlot.Primary, out WeaponSO primaryEquipped) &&
                    !primaryEquipped.AlwaysDrawn)
                    character.Equipment.EquippedWeapons[WeaponSlot.Primary].StoreWeapon(Anim);
                EntityManager.RemoveComponent<StoreWeapon>(entity);
            }).Run();

        }

    }
    public struct DrawWeapon : IComponentData { }
    public struct StoreWeapon : IComponentData { }

}