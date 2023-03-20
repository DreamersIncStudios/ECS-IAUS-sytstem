using AISenses;
using Dreamers.InventorySystem;
using Global.Component;
using IAUS.ECS;
using IAUS.ECS.Component;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DreamersInc.BestiarySystem
{
    public sealed partial class BestiaryDB : MonoBehaviour
    {
        public static void AddIAUS(Entity entity, CreatureInfo info)
        {
            

            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;


            manager.AddComponentData(entity, new AITarget()
            {
                FactionID = 2,// TODO add info.factionID,
                NumOfEntityTargetingMe = 3,
                CanBeTargetByPlayer = false,
                Type = TargetType.Character,
                CenterOffset = new float3(0, 1, 0) //todo add value to SO

            });

            manager.AddComponent<AIStat>(entity);
            manager.AddComponent<IAUSBrain>(entity);
            manager.AddBuffer<ScanPositionBuffer>(entity);
            foreach (var state in info.AIStatesToAdd)
            {
                switch (state)
                {
                    case AIStates.Patrol:
                        var patrol = new Patrol()
                        {
                            NumberOfWayPoints = 10,
                            BufferZone = .25f,
                            _coolDownTime = 5.5f
                        };
                        manager.AddComponentData(entity, patrol);
                        manager.AddBuffer<TravelWaypointBuffer>(entity);
                        break;

                    case AIStates.Traverse:
                        var traverse = new Traverse()
                        {
                            NumberOfWayPoints = 10,
                            BufferZone = .25f,
                            _coolDownTime = 5.5f
                        };
                        manager.AddComponentData(entity, traverse);
                        manager.AddBuffer<TravelWaypointBuffer>(entity);
                        break;

                    case AIStates.Wait:
                        var wait = new Wait()
                        {
                            _coolDownTime = 5.5f
                        };
                        manager.AddComponentData(entity, wait);
                        break;
                    case AIStates.Attack:
                        var attack = new AttackState() {
                        _coolDownTime =5.5f,
                        };
                        manager.AddComponentData(entity, attack);
                        manager.AddComponent<CheckAttackStatus>(entity);
                        manager.AddComponent<MagicAttackSubState>(entity);
                        manager.AddComponent<RangedAttackSubState>(entity);

                        manager.AddComponent<MagicMeleeAttackSubState>(entity);
                        manager.AddComponent<MeleeAttackSubState>(entity);

                        break;

                }
            }
            manager.AddBuffer<StateBuffer>(entity);

            manager.AddComponent<SetupBrainTag>(entity);


        }
        
    }
}