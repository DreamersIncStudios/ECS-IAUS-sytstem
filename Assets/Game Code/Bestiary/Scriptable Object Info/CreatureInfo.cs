using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using IAUS.ECS;
using UnityEditor;
using Global.Component;
using Dreamers.InventorySystem.Base;
using DreamersInc.ComboSystem;
using DreamersIncStudio.GAIACollective;
using MotionSystem.Components;
using IAUS.ECS.Component;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace DreamersInc.BestiarySystem
{
    public class CreatureInfo : ScriptableObject
    {
        [SerializeField] private uint creatureID;

        public uint ID
        {
            get { return creatureID; }
        }

        public string Name;
        public uint ClassLevel;
        public NPCLevel GetNPCLevel;
        public Role Role;
        public CharacterClass stats;
        public GameObject Prefab;
        public TimesOfDay ActiveHours;
        public List<AIStates> AIStatesToAdd;
        public PhysicsInfo PhysicsInfo;
        public MovementData Move;

        [FormerlySerializedAs("factionID")] [Header("influence ")]
        public int FactionID;

        public int BaseThreat;
        public int BaseProtection;
        public EquipmentSave Equipment;
        public NPCAttackSequence AttackSequence;

        [Header("Attack Info")] [ShowIf("hasAttack")]
        public bool CapableOfMelee = false;

        [ShowIf("hasAttack")] public bool CapableOfMagic = false;
        [ShowIf("hasAttack")] public bool CapableOfRange = false;
        public bool hasAttack => AIStatesToAdd.Contains(AIStates.Attack);


#if UNITY_EDITOR
        private void OnValidate()
        {
            // Check if creatureID needs to be reassigned
            if (ShouldReassignID())
            {
                setItemID();
            }
        }

        private bool ShouldReassignID()
        {
            // Use the existing ID to find the current NPC configuration in the database
            var existingCreature = BestiaryDB.GetCreature(creatureID);

            // If no match is found or values have changed, reassign the ID
            return existingCreature == null || existingCreature.GetNPCLevel != GetNPCLevel ||
                   existingCreature.Role != Role;
        }

        public void setItemID()
        {
            uint baseID = GetNPCLevel switch
            {
                NPCLevel.Grunt => 1000,
                NPCLevel.Specialist => 2000,
                NPCLevel.Tower => 3000,
                NPCLevel.NPC => 4000,
                NPCLevel.Daemon => 4000,
                NPCLevel.Beast => 5000,
                NPCLevel.spawner => 6000,
                _ => 0
            };
            // Combine Role and incremental count into the ID
            uint roleModifier = (uint)Role * 100; // Assuming Role enum values are sequentially ordered
            var count = (uint)BestiaryDB.GetCountByCategory(GetNPCLevel, Role);
            this.creatureID = baseID + roleModifier + count;


        }
#endif
    }

#if UNITY_EDITOR
    public static partial class Creator
    {
        [MenuItem("Assets/Create/Bestiary/Creature Info")]
        public static void CreateCreatureInfo()
        {
            Dreamers.Global.ScriptableObjectUtility.CreateAsset<CreatureInfo>("Creature", out CreatureInfo info);
            BestiaryDB.LoadDatabase(true);
            info.setItemID();
        }

        [MenuItem("Assets/Create/Bestiary/Spawn Creature Info")]
        public static void CreateSpawnPackInfo()
        {
            Dreamers.Global.ScriptableObjectUtility.CreateAsset<PackSpawnCreatureInfo>("Creature",
                out PackSpawnCreatureInfo info);
            BestiaryDB.LoadDatabase(true);
            info.setItemID();
        }

    }
#endif
}