using Unity.Entities;
using Dreamers.InventorySystem;
using Dreamers.InventorySystem.Interfaces;

namespace IAUS.ECS.Component {
    public partial class EquipmentUpdate : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().WithStructuralChanges().ForEach(( Entity entity, CharacterInventory test,  ref AttackCapable capable, in CheckAttackStatus tag) => {

               capable.CapableOfMelee = false;
             capable.CapableOfMagic = false;
             capable.CapableOfProjectile = false;
         

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
                                  capable.CapableOfMelee = true;
                            break;
                        case WeaponType.Mage_Staff:
                        case WeaponType.Enchanter_Stone:

                           capable.CapableOfMagic = true;
                            break;
                        case WeaponType.Bow:

                        case WeaponType.Pistol:
                              capable.CapableOfProjectile = true;
                            break;
                    }
                    
                    if (test.Equipment.EquippedAbility.Count > 0)
                        capable.CapableOfMagic = true;
                }
                EntityManager.RemoveComponent<CheckAttackStatus>(entity);
            }).Run();
        
        }
    }
}
