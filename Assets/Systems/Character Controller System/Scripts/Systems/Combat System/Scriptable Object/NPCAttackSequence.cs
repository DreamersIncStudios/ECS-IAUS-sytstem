using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DreamersInc.ComboSystem
{
   [CreateAssetMenu(fileName = "Attack" +
                               "", menuName = "ComboSystem/NPC Attack Sequence")]
   public class NPCAttackSequence : ScriptableObject
   {
      [System.Serializable]
      public class AttackSequence
      {
      
         
         public List<AnimationCombo> Attacks;
         // How many units the item takes - more units, higher chance of being picked
         [Range(0,100)]
         public float ProbabilityWeight;

         // Displayed only as an information for the designer/programmer. Should not be set manually via inspector!    
         public float ProbabilityPercent;

         // These values are assigned via LootDropTable script. They represent from which number to which number if selected, the item will be picked.
         [HideInInspector] 
         public float ProbabilityRangeFrom;
         [HideInInspector] 
         public float ProbabilityRangeTo;   
      }

      public List<AttackSequence> AttackSequences;

      [SerializeField] public float TotalProbabilityWeight;

      public void ValidateTable()
      {

         // Prevent editor from "crying" when the item list is empty :)
         if (AttackSequences is not { Count: > 0 }) return;
         var currentProbabilityWeightMaximum = 0f;

         // Sets the weight ranges of the selected items.
         foreach(var attack in AttackSequences){

            if(attack.ProbabilityWeight < 0f){
               // Prevent usage of negative weight.
               Debug.Log("You can't have negative weight on an item. Resetting item's weight to 0.");
               attack.ProbabilityWeight =  0f;
            } else {
               attack.ProbabilityRangeFrom = currentProbabilityWeightMaximum;
               currentProbabilityWeightMaximum += attack.ProbabilityWeight;	
               attack.ProbabilityRangeTo = currentProbabilityWeightMaximum;						
            }

         }

         TotalProbabilityWeight = currentProbabilityWeightMaximum;

         // Calculate percentage of item drop select rate.
         foreach(var attack in AttackSequences){
            attack.ProbabilityPercent = ((attack.ProbabilityWeight) / TotalProbabilityWeight) * 100;
         }

      }
      public AttackSequence PickLAttackSequence(){		

         var pickedNumber = Random.Range(0, TotalProbabilityWeight);

         // Find an item whose range contains pickedNumber
         foreach (var lootDropItem in AttackSequences)
         {
            // If the picked number matches the item's range, return item
            if(pickedNumber > lootDropItem.ProbabilityRangeFrom && pickedNumber < lootDropItem.ProbabilityRangeTo){
               return lootDropItem;
            }
         }	

         // If item wasn't picked... Notify programmer via console and return the first item from the list
         Debug.LogError("Item couldn't be picked... Be sure that all of your active loot drop tables have assigned at least one item!");
         return AttackSequences[0];
      }
      public AnimationTrigger PickAttack()
      {
         return PickLAttackSequence().Attacks[0].Trigger;
      }
      private void OnValidate()
      {
         ValidateTable();
      }
   }
   public class NPCAttack : IComponentData
   {
      public NPCAttackSequence AttackSequence;
   }
}
