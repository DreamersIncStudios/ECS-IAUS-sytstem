using AISenses;
using Dreamers.InventorySystem.Base;
using DreamersInc.ComboSystem;
using DreamersInc.InfluenceMapSystem;
using Global.Component;
using MotionSystem.Components;
using Stats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using DreamersIncStudio.FactionSystem;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace DreamersInc.BestiarySystem
{

    public class PlayerInfo : ScriptableObject
    {

        [SerializeField] private uint creatureID;
        public uint ID { get { return creatureID; } }
        public string Name;
        public PlayerCharacterClass stats;
        public GameObject Prefab;
        public PhysicsInfo PhysicsInfo;
        public MovementData Move;
        [Header("influence ")]
        public FactionNames factionID;
        public int BaseThreat;
        public int BaseProtection;
        public ComboSO Combo;
        public EquipmentSave Equipment;

#if UNITY_EDITOR

        public void setItemID(uint ID)
        {

            this.creatureID = ID;
        }
#endif
    }

#if UNITY_EDITOR
    public static partial class Creator
    {
        [MenuItem("Assets/Create/Bestiary/Player Info")]
        static public void CreatePlayerInfo()
        {
           Dreamers.Global.ScriptableObjectUtility.CreateAsset<PlayerInfo>("Creature", out PlayerInfo info);
            BestiaryDB.LoadDatabase(true);
            info.setItemID((uint)BestiaryDB.Players.Count + 1);
        }

    }
#endif


}