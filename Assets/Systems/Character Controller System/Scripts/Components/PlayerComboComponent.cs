using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
//using Core.SaveSystems;
using DreamersInc.CombatSystem;

namespace DreamersInc.ComboSystem
{

    public class PlayerComboComponent : IComponentData
    {
        public ComboSO Combo;
        public bool WeaponEquipped { get; set; }
    }
}