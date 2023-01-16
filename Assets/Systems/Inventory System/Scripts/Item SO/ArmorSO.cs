using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using Dreamers.InventorySystem.Base;
using Dreamers.InventorySystem.Interfaces;
using Unity.Entities;
using System.Linq;
using Stats.Entities;
using System;

namespace Dreamers.InventorySystem
{
    [Serializable]
    public class ArmorSO : ItemBaseSO, IEquipable, IArmor
    {
        #region variables
        [SerializeField] Quality quality;
        public Quality Quality { get { return quality; } }

        [SerializeField] private GameObject _model;
        public GameObject Model { get { return _model; } }
        [SerializeField] private bool _equipToHuman;
        public bool EquipToHuman { get { return _equipToHuman; } }
        public bool Equipped { get; private set; }

        [SerializeField] private HumanBodyBones _equipBone;
        public HumanBodyBones EquipBone { get { return _equipBone; } }
        [SerializeField] private ArmorType _armorType;
        public ArmorType ArmorType { get { return _armorType; } }
        [SerializeField] private uint _levelRqd;
        public uint LevelRqd { get { return _levelRqd; } }

        [SerializeField] private List<StatModifier> _modifiers;
        public List<StatModifier> Modifiers { get { return _modifiers; } }

        [SerializeField] private float maxDurable;
        public float MaxDurability { get { return maxDurable; } }
        public float CurrentDurablity { get; set; }
        [SerializeField] private bool breakable;
        public bool Breakable { get { return breakable; } }
        [SerializeField] private bool _upgradeable;
        public bool Upgradeable { get { return _upgradeable; } }

        public int SkillPoints { get; set; }
        public int Exprience { get; set; }
        GameObject armorModel;

        public bool Equip(BaseCharacterComponent player)
        {
            var anim = player.GOrepresentative.GetComponent<Animator>();

            if (player.Level >= LevelRqd)
            {
                if (Model != null)
                {
                    armorModel = _model = Instantiate(Model);
                    // Consider adding and enum as all character maybe not be human 
                    if (EquipToHuman)
                    {
                        Transform bone = anim.GetBoneTransform(EquipBone);
                        if (bone)
                        {
                            armorModel.transform.SetParent(bone);
                        }

                    }
                    else
                    {
                        armorModel.transform.SetParent(anim.transform);

                    }

                }
                player.ModCharacterStats(Modifiers, true);
                return Equipped = true;
            }
            else
            {
                Debug.LogWarning("Level required to Equip is " + LevelRqd + ". Character is currently level " + player.Level);
                return Equipped = false;
            }
        }
        public void Equip(BaseCharacterComponent player,GameObject go) {

            var anim = go ? go.GetComponent<Animator>() : GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();

            //if (Model != null)
            //{
            //    armorModel = _model = Instantiate(Model);
            //    // Consider adding and enum as all character maybe not be human 
            //    if (EquipToHuman)
            //    {
            //        Transform bone = anim.GetBoneTransform(EquipBone);
            //        if (bone)
            //        {
            //            armorModel.transform.SetParent(bone);
            //        }

            //    }
            //    else
            //    {
            //        armorModel.transform.SetParent(anim.transform);

            //    }

            //}
            // player.ModCharacterStats(Modifiers, true);
        }
        #endregion

        /// <summary>
        /// Equip Item in Inventory to Another Character
        /// </summary>
        /// <param name="characterInventory"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public  bool EquipItem(CharacterInventory characterInventory, BaseCharacterComponent player)
        {
            EquipmentBase Equipment = characterInventory.Equipment;
            var Anim = player.GOrepresentative.GetComponent<Animator>();

            if (player.Level >= LevelRqd)
            {
                if (Equipment.EquippedArmor.TryGetValue(this.ArmorType, out _))
                {
                    Equipment.EquippedArmor[this.ArmorType].Unequip(characterInventory, player);
                }
                Equipment.EquippedArmor[this.ArmorType] = this;

                if (Model != null)
                {
                    armorModel = _model = Instantiate(Model);
                    // Consider adding and enum as all character maybe not be human 
                    if (EquipToHuman)
                    {
                        Transform bone =Anim.GetBoneTransform(EquipBone);
                        if (bone)
                        {
                            armorModel.transform.SetParent(bone);
                        }

                    }
                    else
                    {
                        armorModel.transform.SetParent(Anim.transform);

                    }

                }
                player.ModCharacterStats( Modifiers, true);

                characterInventory.Inventory.RemoveFromInventory(this);
                player.StatUpdate();
                return Equipped = true;
            }
            else { Debug.LogWarning("Level required to Equip is " + LevelRqd + ". Character is currently level " + player.Level);
                return Equipped =false;
            }
        }
    

        /// <summary>
        /// Unequip item from character and return to target inventory
        /// </summary>
        /// <param name="characterInventory"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public  bool Unequip(CharacterInventory characterInventory, BaseCharacterComponent player)
        {
            EquipmentBase Equipment = characterInventory.Equipment;
            characterInventory.Inventory.AddToInventory(this);
            Destroy(armorModel);
           player.ModCharacterStats( Modifiers, false);
            Equipment.EquippedArmor.Remove(this.ArmorType);
            Equipped = false;
            return true;
        }

        /// <summary>
        /// Unequip item from self and return inventory
        /// </summary>
        /// <param name="characterInventory"></param>
        /// <returns></returns>


   


        public override void Use(CharacterInventory characterInventory, BaseCharacter player)
        {
            throw new System.NotImplementedException();
        }



        // override object.Equals
        public  bool Equals(ItemBaseSO obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            if (obj.Type != Type)
                return false;

            // TODO: write your implementation of Equals() here

            ArmorSO Armor = (ArmorSO)obj;

            return ItemID == Armor.ItemID  && ItemName == Armor.ItemName && Value == Armor.Value && Modifiers.SequenceEqual( Armor.Modifiers) &&
                Exprience == Armor.Exprience && LevelRqd == Armor.LevelRqd;
        }



    }


    
}