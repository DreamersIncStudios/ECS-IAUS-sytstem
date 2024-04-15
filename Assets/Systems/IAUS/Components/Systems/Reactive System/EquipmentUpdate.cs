using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Dreamers.InventorySystem;
using IAUS.ECS.Component.Aspects;
using Dreamers.InventorySystem.Interfaces;
using IAUS.Core.GOAP;

namespace IAUS.ECS.Component {
    public partial class EquipmentUpdate : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().WithStructuralChanges().ForEach(( Entity entity, CharacterInventory test, AttackGoapAgent goapAgent, ref AttackState attackState, ref CheckAttackStatus tag) => {

               goapAgent.CapableOfMelee= attackState.CapableOfMelee = false;
               goapAgent.CapableOfMagic=attackState.CapableOfMagic = false;
               goapAgent.CapableOfProjectile=attackState.CapableOfProjectile = false;



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
                                goapAgent.CapableOfMelee=      attackState.CapableOfMelee = true;
                            break;
                        case WeaponType.Mage_Staff:
                        case WeaponType.Enchanter_Stone:

                            goapAgent.CapableOfMagic= attackState.CapableOfMagic = true;
                            break;
                        case WeaponType.Bow:

                        case WeaponType.Pistol:
                            goapAgent.CapableOfProjectile=     attackState.CapableOfProjectile = true;
                            break;
                    }
                    
                    if (test.Equipment.EquippedAbility.Count > 0)
                        goapAgent.CapableOfMagic=  attackState.CapableOfMagic = true;
                }

                EntityManager.RemoveComponent<CheckAttackStatus>(entity);
            }).Run();
        }
    }
}
