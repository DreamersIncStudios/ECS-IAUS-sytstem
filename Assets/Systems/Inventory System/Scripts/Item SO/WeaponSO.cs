using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using Dreamers.InventorySystem.Base;
using Dreamers.InventorySystem.Interfaces;
namespace Dreamers.InventorySystem
{
    [CreateAssetMenu(fileName = "Weapon Data", menuName = "Item System/Weapon", order = 2)]
    public class WeaponSO : ItemBaseSO, IEquipable,IWeapon
    {
        public new ItemType Type { get { return ItemType.Weapon; } }
        [SerializeField] Quality quality;
        public Quality Quality { get { return quality; } }
        public EquipmentType Equipment { get { return EquipmentType.Armor; } }
        [SerializeField] GameObject _model;
        public GameObject Model { get { return _model; } }
        [SerializeField] private bool _equipToHuman;
        public bool EquipToHuman { get { return _equipToHuman; } }
        [SerializeField] private HumanBodyBones _equipBone;
        public HumanBodyBones EquipBone { get { return _equipBone; } }
        [SerializeField] private List<StatModifier> _modifiers;
        public List<StatModifier> Modifiers { get { return _modifiers; } }

        [SerializeField] private uint _levelRQD;
        public uint LevelRqd { get { return _levelRQD; } }

        [SerializeField] private WeaponType _weaponType;
        public WeaponType WeaponType { get { return _weaponType; } }
        [SerializeField] private WeaponSlot slot;
        public WeaponSlot Slot { get { return slot; } }
        [SerializeField] private float maxDurable;
        public float MaxDurability { get { return maxDurable; } }
        public float CurrentDurablity { get; set; }
        [SerializeField] private bool breakable;
        public bool Breakable { get { return breakable; } }
        [SerializeField] private bool _upgradeable;
        public bool Upgradeable { get { return _upgradeable; } }

        public int SkillPoints { get; set; }
        public int Exprience { get; set; }

        public override void EquipItem(InventoryBase inventoryBase, EquipmentBase Equipment, int IndexOf,BaseCharacter player)
        {
            if (player.Level >= LevelRqd)
            {
                if (Model != null)
                {
                    GameObject armorModel = Instantiate(Model);
                    // Consider adding and enum as all character maybe not be human 
                    if (EquipToHuman)
                    {
                        Transform bone = player.GetComponent<Animator>().GetBoneTransform(EquipBone);
                        if (bone)
                        {
                            armorModel.transform.SetParent(bone);
                        }

                    }

                }
                        ModCharacterStats(player, true);


                switch (WeaponType)
                {
                    case WeaponType.Bo_Staff:
                    case WeaponType.H2BoardSword:
                    case WeaponType.Axe:
                        switch (Slot)
                        {
                            case WeaponSlot.Primary:
                                if(Equipment.Shield)
                                    Equipment.Shield.Unequip(inventoryBase, Equipment, player, 0);
                                if (Equipment.PrimaryWeapon)
                                    Equipment.PrimaryWeapon.Unequip(inventoryBase, Equipment, player, 0);
                                Equipment.PrimaryWeapon = this;
                                break;
                            case WeaponSlot.Secondary:
                                if (Equipment.SecondaryWeapon)
                                    Equipment.SecondaryWeapon.Unequip(inventoryBase, Equipment, player, 0);
                                Equipment.PrimaryWeapon = this;
                                break;
                        }


                        break;
                    case WeaponType.Sword:
                        switch (Slot)
                        {
                            case WeaponSlot.Primary:
                                if (Equipment.PrimaryWeapon)
                                    Equipment.PrimaryWeapon.Unequip(inventoryBase, Equipment, player, 0);
                                Equipment.PrimaryWeapon = this;
                                break;
                            case WeaponSlot.Secondary:
                                if (Equipment.SecondaryWeapon)
                                    Equipment.SecondaryWeapon.Unequip(inventoryBase, Equipment, player, 0);
                                Equipment.PrimaryWeapon = this;
                                break;
                        }
                        break;
                   
                }
                RemoveFromInventory(inventoryBase, IndexOf);

            }
            else { Debug.LogWarning("Level required to Equip is " + LevelRqd + ". Character is currently level " + player.Level); }

        }

        public override void Unequip(InventoryBase inventoryBase, EquipmentBase Equipment, BaseCharacter player, int IndexOf)
        {
            ModCharacterStats(player, false);
            AddToInventory(inventoryBase);
            switch (Slot)
            {
                case WeaponSlot.Primary:
                    Equipment.PrimaryWeapon = null;
                    break;
                case WeaponSlot.Secondary:
                    Equipment.SecondaryWeapon = null;
                    break;
                case WeaponSlot.Projectile:
                    Equipment.ProjectileWeopon = null;
                    break;
            }

        }

        public override void Use(InventoryBase inventoryBase, int IndexOf, BaseCharacter player)
        {
            throw new System.NotImplementedException();
        }


        //Remove previous Mod and subtract bool
        void ModCharacterStats(BaseCharacter character, bool Add)
        {
            int MP = 1;  
            if (!Add) {
                MP = -1;
                }
            foreach (StatModifier mod in Modifiers)
            {
              
                switch (mod.Stat)
                {
                    case AttributeName.Level:
                        Debug.LogWarning("Level Modding is not allowed at this time. Please contact Programming is needed");
                        break;
                    case AttributeName.Strength:
                        character.GetPrimaryAttribute((int)AttributeName.Strength).BuffValue += mod.BuffValue*MP;
                        break;
                    case AttributeName.Vitality:
                        character.GetPrimaryAttribute((int)AttributeName.Vitality).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Awareness:
                        character.GetPrimaryAttribute((int)AttributeName.Awareness).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Speed:
                        character.GetPrimaryAttribute((int)AttributeName.Speed).BuffValue += mod.BuffValue * MP * MP;
                        break;
                    case AttributeName.Skill:
                        character.GetPrimaryAttribute((int)AttributeName.Skill).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Resistance:
                        character.GetPrimaryAttribute((int)AttributeName.Resistance).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Concentration:
                        character.GetPrimaryAttribute((int)AttributeName.Concentration).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.WillPower:
                        character.GetPrimaryAttribute((int)AttributeName.WillPower).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Charisma:
                        character.GetPrimaryAttribute((int)AttributeName.Charisma).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Luck:
                        character.GetPrimaryAttribute((int)AttributeName.Luck).BuffValue += mod.BuffValue * MP;
                        break;
                }
            }
            character.StatUpdate();

        }
    }

    
}