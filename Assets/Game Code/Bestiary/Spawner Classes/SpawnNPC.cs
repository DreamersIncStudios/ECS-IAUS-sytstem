using AISenses;
using AISenses.VisionSystems.Combat;
using Components.MovementSystem;
using Dreamers.InventorySystem;
using Dreamers.InventorySystem.Base;
using DreamersInc.CombatSystem;
using DreamersInc.ComboSystem;
using DreamersInc.InflunceMapSystem;
using Global.Component;
using MotionSystem;
using MotionSystem.Components;
using Stats;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace DreamersInc.BestiarySystem
{
    public sealed partial class BestiaryDB : MonoBehaviour
    {
        public static bool SpawnNPC(uint ID, out GameObject go, out Entity entity)
        {
            var info = GetCreature(ID);
            if (info != null)
            {
                go = Instantiate(info.Prefab);
                go.tag = "Enemy NPC";
                go.layer= 9;
                EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                entity = CreateEntity(manager, go.transform, info.Name+" NPC");
                  AddPhysics(manager, entity, go, info.PhysicsInfo);
                BaseCharacterComponent character = new()
                {
                    GOrepresentative = go
                };
                character.SetupDataEntity(info.stats);
                TransformGO transformLink = new()
                {
                    transform = go.transform
                };
                var vision = new Vision();
                vision.InitializeSense(character);
                manager.AddComponentData(entity, vision);
                manager.AddComponentData(entity, transformLink);
                manager.AddComponentObject(entity, character);

                CharacterInventory inventory = new();
                inventory.Setup(info.Equipment, character);
                manager.AddComponentData(entity, inventory);
             //   var anim = go.GetComponent<Animator>();
                var RB = go.GetComponent<Rigidbody>();
                manager.AddComponentObject(entity, RB);
                //manager.AddComponentData(entity, new AnimatorComponent()
                //{
                //    anim = anim,
                //    RB = RB,
                //    transform = anim.transform,
                //});
              //  manager.AddComponent<StoreWeapon>(entity);
                manager.AddComponentData(entity, new InfluenceComponent
                {
                    factionID = info.FactionID,
                    Protection = info.BaseProtection,
                    Threat = info.BaseThreat
                });
                manager.AddComponentData(entity, new AITarget()
                {
                    FactionID = info.FactionID,
                    NumOfEntityTargetingMe = 3,
                    CanBeTargetByPlayer = true,
                    Type = TargetType.Character,
                    level = info.ClassLevel,
                    CenterOffset = new float3(0, 1, 0) //todo add value to SO
                }) ;

                var agent = go.GetComponent<NavMeshAgent>();
                manager.AddComponentObject(entity, agent);
                var move = new Movement()
                {
                    Acceleration = agent.acceleration,
                    StoppingDistance = agent.stoppingDistance,
                    Offset = agent.baseOffset,
                };
                move.SetMovementSpeed(character.GetPrimaryAttribute((int)AttributeName.Speed).AdjustBaseValue);
                manager.AddComponentData(entity, move);

                AddIAUS(entity, info,go);
               
            }
            else
            {
                go = null;
                entity = Entity.Null;
            }
            return info != null;

        }


        public static bool SpawnNPC(uint ID, EquipmentSave equipment = null)
        {
            if (SpawnNPC(ID, out GameObject go, out Entity entity))
            {
                EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                var info = GetCreature(ID);

                manager.AddComponent<AttackTarget>(entity);
                manager.AddComponentObject(entity, new Command());
                var controllerData = new CharControllerE();
                controllerData.Setup(info.Move, go.GetComponent<UnityEngine.CapsuleCollider>());
                manager.AddComponentData(entity, controllerData);
                //var comboInfo = Object.Instantiate(info.Combo);
                //manager.AddComponentObject(entity, new PlayerComboComponent { Combo = comboInfo });
                manager.AddComponentData(entity, new InfluenceComponent
                {
                    factionID = info.FactionID,
                    Protection = info.BaseProtection,
                    Threat = info.BaseThreat
                });
                manager.AddComponentData(entity, new Perceptibility
                {
                    movement = MovementStates.Standing_Still,
                    noiseState = NoiseState.Normal,
                    visibilityStates = VisibilityStates.Visible
                });
                manager.AddBuffer<ScanPositionBuffer>(entity);

                //   go.GetComponent<VFXControl>().Init(info.Combo);
                AddIAUS(entity, info, go);
                var agent = go.GetComponent<NavMeshAgent>();
                manager.AddComponentObject(entity, agent);

                return true;
            }
            else
                return false;
        }

        public static bool SpawnNPC(uint ID, out GameObject go, EquipmentSave equipment = null)
        {
            if (SpawnNPC(ID, out go, out Entity entity))
            {
                EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                var info = GetCreature(ID);
                manager.AddComponent<AttackTarget>(entity);
                manager.AddComponentObject(entity, new Command());
                var controllerData = new CharControllerE();
                controllerData.Setup(info.Move, go.GetComponent<UnityEngine.CapsuleCollider>());
                manager.AddComponentData(entity, controllerData);
             //   var comboInfo = Object.Instantiate(info.Combo);
             //   manager.AddComponentObject(entity, new PlayerComboComponent { Combo = comboInfo });
                manager.AddComponentData(entity, new InfluenceComponent
                {
                    factionID = info.FactionID,
                    Protection = info.BaseProtection,
                    Threat = info.BaseThreat
                });
                manager.AddComponentData(entity, new Perceptibility
                {
                    movement = MovementStates.Standing_Still,
                    noiseState = NoiseState.Normal,
                    visibilityStates = VisibilityStates.Visible
                });
                manager.AddBuffer<ScanPositionBuffer>(entity);

                //  go.GetComponent<VFXControl>().Init(info.Combo);
                AddIAUS(entity, info, go);

                var agent = go.GetComponent<NavMeshAgent>();
                manager.AddComponentObject(entity, agent);
      
                return true;
            }
            else
                return false;
        }


        public static bool SpawnNPC(uint ID, Vector3 Position, EquipmentSave equipment = null)
        {
            if (SpawnNPC(ID, out GameObject go, equipment))
            {
                go.transform.position = Position;
                return true;
            }
            else { return false; }
        }

    }
}

