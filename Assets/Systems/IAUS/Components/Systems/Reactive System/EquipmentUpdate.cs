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
            Entities.WithoutBurst().WithStructuralChanges().ForEach(( Entity entity, CharacterInventory test, ref AttackState attackState, ref CheckAttackStatus tag) => {

                attackState.CapableOfMelee = false;
                attackState.CapableOfMagic = false;
                attackState.CapableOfProjectile = false;



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
                            case WeaponType.Claws:
                            attackState.CapableOfMelee = true;
                            break;
                        case WeaponType.Mage_Staff:
                        case WeaponType.Enchanter_Stone:

                            attackState.CapableOfMagic = true;
                            break;
                        case WeaponType.Bow:

                        case WeaponType.Pistol:
                            attackState.CapableOfProjectile = true;
                            break;
                    }
                    
                    if (test.Equipment.EquippedAbility.Count > 0)
                        attackState.CapableOfMagic = true;
                }

                EntityManager.RemoveComponent<CheckAttackStatus>(entity);
            }).Run();
        }
    }
}
