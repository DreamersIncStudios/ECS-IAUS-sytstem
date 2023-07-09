using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using IAUS.ECS;
using UnityEditor;
using Global.Component;
using Dreamers.InventorySystem.Base;
using MotionSystem.Components;

namespace DreamersInc.BestiarySystem
{
    public class CreatureInfo : ScriptableObject
    {
        [SerializeField] private uint creatureID;
        public uint ID { get { return creatureID; } }
        public string Name;
        public uint ClassLevel;
        public CharacterClass stats;
        public GameObject Prefab;
        public List<AIStates> AIStatesToAdd;
        public PhysicsInfo PhysicsInfo;
        public MovementData Move;
        [Header("influence ")]
        public int factionID;
        public int BaseThreat;
        public int BaseProtection;
        public EquipmentSave Equipment;


#if UNITY_EDITOR

        public void setItemID(uint ID)
        {

            this.creatureID = ID;
        }
#endif
    }

#if UNITY_EDITOR
    public static partial class Creator {
        [MenuItem("Assets/Create/Creature Info")]
        static public void CreateCreatureInfo() {
            Dreamers.Global.ScriptableObjectUtility.CreateAsset<CreatureInfo>("Creature", out CreatureInfo info);
            BestiaryDB.LoadDatabase(true);
            info.setItemID((uint)BestiaryDB.Creatures.Count + 1);
        }

    }
#endif
}