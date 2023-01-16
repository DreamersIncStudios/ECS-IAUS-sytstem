using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dreamers.InventorySystem
{

    public interface IGeneral
    {
        TypeOfGeneralItem GeneralItemType{get;}
    }
    public enum TypeOfGeneralItem { 
        Health, Mana, Attack, Status, Key, 
    
    }
}