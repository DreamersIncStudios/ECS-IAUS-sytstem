using DreamersInc.ComboSystem;
using System.Collections.Generic;
using Unity.Entities;

namespace Dreamers.InventorySystem.AbilitySystem
{
    public partial class AbilityUpdateSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithoutBurst().ForEach((Entity entity, CharacterInventory inventory, Command handler, ref UpdateCommandHandlerTag tag) => {

                handler.EquippedAbilities.EquippedAbilities = new List<AbilitySO>();

                foreach (AbilitySO so in inventory.Equipment.EquippedAbility) { 
                    handler.EquippedAbilities.EquippedAbilities.Add(so);
                }

                EntityManager.RemoveComponent<UpdateCommandHandlerTag>(entity);
            }).WithStructuralChanges().Run();
        }
    }
}
