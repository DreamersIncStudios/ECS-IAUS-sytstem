using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DreamersInc.ComboSystem
{
   [CreateAssetMenu(fileName = "Attack" +
                               "", menuName = "ComboSystem/NPC Attack Sequence")]
   public class NPCAttackSequence : ScriptableObject
   {
      [SerializeReference]public List<IAttackSequence> AttackSequences;

      [SerializeField] public float TotalProbabilityWeightMelee;
      [SerializeField] public float TotalProbabilityWeightMagic;
      [SerializeField] public float TotalProbabilityWeightProjectile;
      public void ValidateTable(IAttackSequence.AttackType Type)
      {

         // Prevent editor from "crying" when the item list is empty :)
         if (AttackSequences is not { Count: > 0 }) return;
         var currentProbabilityWeightMaximum = 0f;

         // Sets the weight ranges of the selected items.
         foreach(var attack in AttackSequences){
            if(attack.Type != Type) continue;
            if(attack.ProbabilityWeight < 0f){
               // Prevent usage of negative weight.
               Debug.Log("You can't have negative weight on an item. Resetting item's weight to 0.");
            } else {
               attack.ProbabilityRangeFrom = currentProbabilityWeightMaximum;
               currentProbabilityWeightMaximum += attack.ProbabilityWeight;	
               attack.ProbabilityRangeTo = currentProbabilityWeightMaximum;						
            }

         }

         switch (Type)
         {
            case IAttackSequence.AttackType.Melee:
               TotalProbabilityWeightMelee = currentProbabilityWeightMaximum;
               // Calculate percentage of item drop select rate.
               foreach (var attack in AttackSequences)
               {
                  if(attack.Type!= Type) continue;
                  attack.ProbabilityPercent = ((attack.ProbabilityWeight) / TotalProbabilityWeightMelee) * 100;
               }
               break;
            
            case IAttackSequence.AttackType.Magic:
               TotalProbabilityWeightMagic = currentProbabilityWeightMaximum;
               // Calculate percentage of item drop select rate.
               foreach (var attack in AttackSequences)
               {
                  if(attack.Type!= Type) continue;
                  attack.ProbabilityPercent = ((attack.ProbabilityWeight) / TotalProbabilityWeightMagic) * 100;
               }
               break;
            
            case IAttackSequence.AttackType.Projectile:
               TotalProbabilityWeightProjectile = currentProbabilityWeightMaximum;
               // Calculate percentage of item drop select rate.
               foreach (var attack in AttackSequences)
               {           
                  if(attack.Type!= Type) continue;
                  attack.ProbabilityPercent = ((attack.ProbabilityWeight) / TotalProbabilityWeightProjectile) * 100;
               }
               break;
         }

      }
      public IAttackSequence PickLAttackSequence(IAttackSequence.AttackType Type)
      {
         var pickedNumber = new float();
         switch (Type)
         {
            case IAttackSequence.AttackType.Melee:
               pickedNumber = Random.Range(0, TotalProbabilityWeightMelee);
               break;
            case IAttackSequence.AttackType.Magic:
               pickedNumber = Random.Range(0, TotalProbabilityWeightMagic);
               break;
            case IAttackSequence.AttackType.Projectile:
               pickedNumber = Random.Range(0, TotalProbabilityWeightProjectile);
               break;
         }
         

         // Find an item whose range contains pickedNumber
         foreach (var lootDropItem in AttackSequences)
         {
            if (lootDropItem.Type != Type) continue;
            // If the picked number matches the item's range, return item
            if (pickedNumber > lootDropItem.ProbabilityRangeFrom && pickedNumber < lootDropItem.ProbabilityRangeTo)
            {
               return lootDropItem;
            }
         }	

         // If item wasn't picked... Notify programmer via console and return the first item from the list
         Debug.LogError("Item couldn't be picked... Be sure that all of your active loot drop tables have assigned at least one item!");
         return AttackSequences[0];
      }
      public List<AnimationTrigger> PickAttack(IAttackSequence.AttackType Type)
      {
         return PickLAttackSequence(Type).Triggers;
      }
      private void OnValidate()
      {
         ValidateTable(IAttackSequence.AttackType.Melee);
         ValidateTable(IAttackSequence.AttackType.Magic);
         ValidateTable(IAttackSequence.AttackType.Projectile);

      }
   }

}
