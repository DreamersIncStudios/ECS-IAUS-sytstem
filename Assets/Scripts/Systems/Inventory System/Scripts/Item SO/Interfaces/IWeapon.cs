using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Dreamers.InventorySystem
{
    public interface IWeapon
    {
        WeaponType WeaponType { get; }
        WeaponSlot Slot { get; }
        float MaxDurability { get; }
        float CurrentDurablity { get; set; }
        bool Breakable { get; }
        bool Upgradeable { get; }
        int SkillPoints { get; set; }
        int Exprience { get; set; }

        //List skills SOs
    }
    public enum WeaponType { 
        Sword, H2BoardSword, Katana, Bo_Staff, Mage_Staff, Club, Pistol, Bow, Axe, Gloves, Enchanter_Stone
    }
    public enum WeaponSlot { Primary, Secondary, Projectile}
}
