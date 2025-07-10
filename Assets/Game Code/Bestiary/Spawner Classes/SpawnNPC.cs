
using System;
using DreamersIncStudio.GAIACollective;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace DreamersInc.BestiarySystem
{
    public sealed partial class BestiaryDB : MonoBehaviour
    {
        public static bool SpawnNPC(uint ID, Vector3 Position, out GameObject GO, out Entity entity) {
            
            var info = GetCreature(ID);
            if (!info) throw new AssertionException(nameof(ID), $"ID {ID} not valid entry in Database");
            switch (info.Role)
            {
                case Role.Recon:
                    break;
                case Role.Combat:
                    new CharacterBuilder(info.Name, out entity)
                        .WithModel(info.Prefab, Position, "Enemy NPC", out GO)
                        .WithStats(info.stats)
                        .WithEntityPhysics(info.PhysicsInfo)
                        // .WithInventorySystem(info.Inventory, info.Equipment)
                        .WithAIControl()
                        .WithCharacterDetection()
                        .WithAnimation()
                        .WithNPCAttack(info.AttackSequence)
                        .WithMovement(info.Move)
                        .WithFactionInfluence(info.FactionID, 3, 4, 1, true)
                        .WithAI(info.GetNPCLevel, info.AIStatesToAdd, info.CapableOfMelee, info.CapableOfMagic,
                            info.CapableOfRange)
                        .Build();
                    return true;
                case Role.Scavengers:
                    break;
                case Role.Transport:
                    break;
                case Role.Acquisition:
                    break;
                case Role.Support:
                    new CharacterBuilder(info.Name, out entity)
                        .WithModel(info.Prefab, Position, "Enemy NPC", out GO)
                        .WithStats(info.stats)
                        .WithEntityPhysics(info.PhysicsInfo)
                        // .WithInventorySystem(info.Inventory, info.Equipment)
                        .WithAIControl()
                        .WithCharacterDetection()
                        .WithAnimation()
                        .WithMovement(info.Move)
                        .WithFactionInfluence(info.FactionID, 3, 4, 1, true)
                        .WithAI(info.GetNPCLevel, info.AIStatesToAdd, info.CapableOfMelee, info.CapableOfMagic,
                            info.CapableOfRange)
                        .Build();
                    return true;

                case Role.Spawner: 
                    
                    var packInfo = (PackSpawnCreatureInfo)GetCreature(ID);
                    if (packInfo.PackType== PackType.Special)
                    {
                        new CharacterBuilder(info.Name, out entity)
                            .WithModel(info.Prefab, Position, "Enemy NPC", out GO)
                            .WithStats(info.stats)
                            .WithEntityPhysics(info.PhysicsInfo)
                            // .WithInventorySystem(info.Inventory, info.Equipment)
                            .WithAIControl()
                            .WithCharacterDetection()
                            .WithAnimation()
                            .WithNPCAttack(info.AttackSequence)
                            .WithMovement(info.Move)
                            .WithFactionInfluence(info.FactionID, 3, 4, 1, true)
                            .WithAI(info.GetNPCLevel, info.AIStatesToAdd, info.CapableOfMelee, info.CapableOfMagic,
                                info.CapableOfRange)
                            .WithPackSpawning(packInfo.RequiredPackRoles)
                            .Build();
                    }
                    else
                    {
                        new CharacterBuilder(info.Name, out entity)
                            .WithModel(info.Prefab, Position, "Enemy NPC", out GO)
                            .WithStats(info.stats)
                            .WithEntityPhysics(info.PhysicsInfo)
                            // .WithInventorySystem(info.Inventory, info.Equipment)
                            .WithAIControl()
                            .WithCharacterDetection()
                            .WithAnimation()
                            .WithNPCAttack(info.AttackSequence)
                            .WithMovement(info.Move)
                            .WithFactionInfluence(info.FactionID, 3, 4, 1, true)
                            .WithAI(info.GetNPCLevel, info.AIStatesToAdd, info.CapableOfMelee, info.CapableOfMagic,
                                info.CapableOfRange)
                            .WithPackSpawning(packInfo.PackType)

                            .Build();
                    }

                    return true;
                    break;
                default:
                    new CharacterBuilder(info.Name, out entity)
                        .WithModel(info.Prefab, Position, "Enemy NPC", out GO)
                        .WithStats(info.stats)
                        .WithEntityPhysics(info.PhysicsInfo)
                        // .WithInventorySystem(info.Inventory, info.Equipment)
                        .WithAIControl()
                        .WithCharacterDetection()
                        .WithAnimation()
                        .WithMovement(info.Move)
                        .WithFactionInfluence(info.FactionID, 3, 4, 1, true)
                        .WithAI(info.GetNPCLevel, info.AIStatesToAdd, info.CapableOfMelee, info.CapableOfMagic,
                            info.CapableOfRange)
                        .Build();
                    return true;

            }

            entity = Entity.Null;
            GO = null;
            
            return false;

        }
        public static bool SpawnNPC(uint ID, Vector3 Position, out GameObject GO)
        {
            return SpawnNPC(ID, Position, out GO, out _);
        } 
        
        public static bool SpawnNPC(uint ID, Vector3 Position) {
            return SpawnNPC(ID, Position, out _, out _);
        }

   


    }
}

