using Components.MovementSystem;
using DreamersInc.BestiarySystem.Testing;
using Global.Component;
using MotionSystem;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace DreamersInc.BestiarySystem
{
    public sealed partial class BestiaryDB : MonoBehaviour
    {
        //Text file or ScriptableOjects;
        static public List<CreatureInfo> Creatures;
        static public List<PlayerInfo> Players;
        static public List<TestingDummyInfo> Dummies;

        static public bool IsLoaded { get; private set; }
        private static void ValidateDatabase()
        {
            if (Creatures == null || !IsLoaded)
            {
                Creatures = new();
                Players = new();
                Dummies = new();
                IsLoaded = false;
            }
            else
            {
                IsLoaded = true;
            }
        }

        public static void LoadDatabase(bool ForceLoad = false)
        {

            if (IsLoaded && !ForceLoad)
                return;
            Creatures = new List<CreatureInfo>();
            CreatureInfo[] creatureSO = Resources.LoadAll<CreatureInfo>(@"Creatures");
            foreach (var item in creatureSO)
            {
                if (!Creatures.Contains(item))
                    Creatures.Add(item);
            }

            Players = new();
            PlayerInfo[] playerSO = Resources.LoadAll<PlayerInfo>(@"Player Characters");
            foreach (var item in playerSO)
            {
                if (!Players.Contains(item))
                    Players.Add(item);
            }

            Dummies = new();
            TestingDummyInfo[] testingSO = Resources.LoadAll<TestingDummyInfo>(@"Testing");
            Debug.Log(testingSO.Length);
            foreach (var item in testingSO)
            {
                if (!Dummies.Contains(item))
                    Dummies.Add(item);
            }

            IsLoaded = true;
        }

        public static void ClearDatabase()
        {
            IsLoaded = false;
            Creatures.Clear();

        }
        public static CreatureInfo GetCreature(uint id)
        {
            ValidateDatabase();
            LoadDatabase();
            foreach (CreatureInfo creature in Creatures)
            {
                if (creature.ID == id) return creature;

            }
            return null;
        }

        public static PlayerInfo GetPlayer(uint id)
        {
            ValidateDatabase();
            LoadDatabase();
            foreach (var player in Players)
            {
                if (player.ID == id)
                    return player;
            }
            return null;
        }

        public static TestingDummyInfo GetDummy(uint id) {
            ValidateDatabase();
            LoadDatabase();
            foreach(var dummy in Dummies)
            {
                if(dummy.ID == id)
                    return dummy;
            }
            return null;
        }
    }
    public enum PhysicsShape { Box, Capsule, Sphere, Cyclinder, Custom }
}