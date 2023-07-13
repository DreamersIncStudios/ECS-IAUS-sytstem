using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Dreamers.InventorySystem;
using IAUS.ECS.Component.Aspects;
using Dreamers.InventorySystem.Interfaces;

namespace IAUS.ECS.Component {
    public partial class EquipmentUpdate : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().WithStructuralChanges().ForEach(( Entity entity, CharacterInventory test, ref AttackState aspect, ref CheckAttackStatus tag) => {

                aspect.CapableOfMelee = false;
                aspect.CapableOfMagic = false;
                aspect.CapableOfProjectile = false;



                foreach (var item in test.Equipment.EquippedWeapons)
                {
                    switch (item.Value.WeaponType)
                    {
                        case WeaponType.Axe:
                        case WeaponType.Sword:
                        case WeaponType.H2BoardSword:
                        case WeaponType.Katana:
                        case WeaponType.Bo_Staff:
                        case WeaponType.Club:
                        case WeaponType.Gloves:
                            aspect.CapableOfMelee = true;
                            break;

                        case WeaponType.Mage_Staff:
                        case WeaponType.Enchanter_Stone:

                            aspect.CapableOfMagic = true;
                            break;
                        case WeaponType.Bow:

                        case WeaponType.Pistol:
                            aspect.CapableOfProjectile = true;
                            break;
                    }
                }

                EntityManager.RemoveComponent<CheckAttackStatus>(entity);
            }).Run();
        }
    }
}
