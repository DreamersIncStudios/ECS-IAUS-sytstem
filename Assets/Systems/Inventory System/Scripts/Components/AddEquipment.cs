using Dreamers.InventorySystem.Interfaces;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


namespace Dreamers.InventorySystem
{
    public class AddEquipment : IComponentData
    {
        public IEquipable equipItem;
    } }
