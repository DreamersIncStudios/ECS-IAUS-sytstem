
using System;
using IAUS.ECS.Component;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace DreamersInc.BestiarySystem
{
    public sealed partial class BestiaryDB : MonoBehaviour
    {
        private static bool SpawnNPC(uint ID, Vector3 Position, uint HomeBiomeID, uint PlayerLevel, Entity parentToLink, out GameObject GO, out Entity entity)
        {

            var info = GetCreature(ID);
            if (!info) throw new AssertionException(nameof(ID), $"ID {ID} not valid entry in Database");

            switch (info.GetNPCLevel)
            {
                case NPCLevel.Grunt:
                    new CharacterBuilder(info.Name, out entity)
                        .WithModel(info.Prefab, Position, "Enemy NPC", out GO)
                        .WithStats(info.stats, PlayerLevel ,info.Name)
                        .WithEntityPhysics(info.PhysicsInfo)
                        .WithActiveHour(info.ActiveTimesOfDay,HomeBiomeID)
                        .WithParent(parentToLink)
                        // .WithInventorySystem(info.Inventory, info.Equipment)
                        .WithAIControl()
                        .WithCharacterDetection()
                        .WithAnimation()
                        .WithNPCAttack(info.AttackSequence)
                        .WithMovement(info.Move)
                        .WithFactionInfluence(info.FactionID, 3, info.ClassLevel, info.CenterOffset ,true)
                        .WithAI(info.GetNPCLevel, info.AIStatesToAdd, info.CapableOfMelee, info.CapableOfMagic,
                            info.CapableOfRange,info.Role)
                        .Build();
                    return true;
                case NPCLevel.Specialist:
                    break;
                case NPCLevel.Tower:
                    break;
                case NPCLevel.NPC:

                    new CharacterBuilder(info.Name, out entity)
                        .WithModel(info.Prefab, Position, "Enemy NPC", out GO)
                        .WithStats(info.stats, PlayerLevel,  info.Name)
                        .WithEntityPhysics(info.PhysicsInfo)
                        // .WithInventorySystem(info.Inventory, info.Equipment)
                        .WithAIControl()
                        .WithCharacterDetection()
                        .WithAnimation()
                        .WithMovement(info.Move)
                        .WithFactionInfluence(info.FactionID, 3, info.ClassLevel,  info.CenterOffset,true)
                        .WithAI(info.GetNPCLevel, info.AIStatesToAdd, info.CapableOfMelee, info.CapableOfMagic,
                            info.CapableOfRange,info.Role)
                        .Build();
                    return true;
                case NPCLevel.Daemon:
                    new CharacterBuilder(info.Name, out entity)
                        .WithModel(info.Prefab, Position, "Enemy NPC", out GO)
                        .WithStats(info.stats, PlayerLevel ,info.Name)
                        .WithEntityPhysics(info.PhysicsInfo)
                        .WithActiveHour(info.ActiveTimesOfDay,HomeBiomeID)
                        .WithParent(parentToLink)
                        // .WithInventorySystem(info.Inventory, info.Equipment)
                        .WithAIControl()
                        .WithCharacterDetection()
                        .WithAnimation()
                        .WithNPCAttack(info.AttackSequence)
                        .WithMovement(info.Move)
                        .WithFactionInfluence(info.FactionID, 3, info.ClassLevel, info.CenterOffset,true)
                        .WithAI(info.GetNPCLevel, info.AIStatesToAdd, info.CapableOfMelee, info.CapableOfMagic,
                            info.CapableOfRange,info.Role)
                        .Build();
                    break;
                case NPCLevel.Beast:
                    break;
                case NPCLevel.Spawner:
                    var packInfo = (PackSpawnCreatureInfo)info;
                    new CharacterBuilder(info.Name, out entity)
                        .WithModel(info.Prefab, Position, "Spawner NPC", out GO)
                        .WithStats(info.stats,PlayerLevel,  info.Name)
                        .WithEntityPhysics(info.PhysicsInfo)
                        .WithAIControl()
                        .WithCharacterDetection()
                        .WithAnimation()
                        .WithMovement(info.Move)
                        .WithFactionInfluence(info.FactionID, 3, info.ClassLevel, info.CenterOffset, true)
                        .WithAI(info.GetNPCLevel, info.AIStatesToAdd, info.CapableOfMelee, info.CapableOfMagic,
                            info.CapableOfRange,packInfo.Role)
                        .Build();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            GO = null;
            entity = Entity.Null;
            return false;
        }



        public static bool SpawnNPC(uint ID, Vector3 Position, uint HomeBiomeID, uint PlayerLevel,  out GameObject GO)
        {
            return SpawnNPC(ID, Position, HomeBiomeID,PlayerLevel,Entity.Null,  out GO, out _);
        } 
        
        public static bool SpawnNPC(uint ID, Vector3 Position, uint HomeBiomeID, uint PlayerLevel ) {
            return SpawnNPC(ID, Position, HomeBiomeID,PlayerLevel,Entity.Null, out _, out _);
        }     
        public static bool SpawnNPC(uint ID, Vector3 Position, uint HomeBiomeID, uint PlayerLevel, Entity parentToLink ) {
            return SpawnNPC(ID, Position, HomeBiomeID,PlayerLevel,parentToLink, out _, out _);
        }

    }
}

