using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using IAUS.ECS;
using UnityEditor;
using Global.Component;
using Dreamers.InventorySystem.Base;
using DreamersInc.ComboSystem;
using MotionSystem.Components;
using IAUS.ECS.Component;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace DreamersInc.BestiarySystem
{
    public class CreatureInfo : ScriptableObject
    {
        [SerializeField] private uint creatureID;
        public uint ID { get { return creatureID; } }
        public string Name;
        public uint ClassLevel;
        public NPCLevel GetNPCLevel;
        public CharacterClass stats;
        public GameObject Prefab;
        public List<AIStates> AIStatesToAdd;
        public PhysicsInfo PhysicsInfo;
        public MovementData Move;
        [FormerlySerializedAs("factionID")] [Header("influence ")]
        public int FactionID;
        public int BaseThreat;
        public int BaseProtection;
        public EquipmentSave Equipment;
        public ComboSO AttackComboSO;

        [Header("Attack Info")] [ShowIf("hasAttack")]
        public bool CapableOfMelee;

        [ShowIf("hasAttack")] public bool CapableOfMagic;
        [ShowIf("hasAttack")]public bool CapableOfRange;
        private bool hasAttack => AIStatesToAdd.Contains(AIStates.Attack);
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