using AISenses;
using Dreamers.InventorySystem;
using Global.Component;
using IAUS.ECS;
using IAUS.ECS.Component;
using Stats.Entities;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DreamersInc.BestiarySystem
{
    public sealed partial class BestiaryDB 
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once IdentifierTypo
        private static void AddIAUS(Entity entity, CreatureInfo info, GameObject go)
        {
            

            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;


            manager.AddComponentData(entity, new AITarget()
            {
                FactionID = info.FactionID,
                NumOfEntityTargetingMe = 3,
                CanBeTargetByPlayer = false,
                Type = TargetType.Character,
                CenterOffset = new float3(0, 1, 0), //todo add value to SO
                level = info.ClassLevel
            });

            manager.AddComponentData(entity, new AIStat(10));
            manager.AddComponentData(entity, new IAUSBrain()
            {
                NPCLevel= info.GetNPCLevel,
                FactionID = info.FactionID
            });
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
                        if (info.ClassLevel > 3)
                            patrol.StayInQuadrant = true;
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
                    case AIStates.WanderQuadrant:
                        var wander = new WanderQuadrant()
                        {
                            SpawnPosition = go.transform.position,
                            _coolDownTime = 5.5f,
                            BufferZone = .25f,
                            //WanderNeighborQuadrants = true
                            //TODO Figure out way above line causes issues

                        };
                        manager.AddComponentData(entity, wander);

                        break;
                    case AIStates.Wait:
                        var wait = new Wait()
                        {
                            _coolDownTime = 5.5f
                        };
                        manager.AddComponentData(entity, wait);
                        break;
                    case AIStates.Attack:
                        manager.AddComponentData(entity, new AttackState(5.5f,true,false,false));
                        manager.AddComponent<CheckAttackStatus>(entity);
                        manager.AddComponent<MagicAttackSubState>(entity);
                        manager.AddComponentData(entity, new RangedAttackSubState() { 
                            MaxEffectiveRange = 60,
                        });

                        manager.AddComponent<MagicMeleeAttackSubState>(entity);
                        var melee = new MeleeAttackSubState();
                        if(info.AttackComboSO)
                            melee.SetupPossibleAttacks(info.AttackComboSO);
                        manager.AddComponentData(entity, melee);

                        break;
                    case AIStates.RetreatToLocation:
          
                        manager.AddComponentData(entity, new EscapeThreat(coolDownTime: 10f));
                        break;
                    case AIStates.RetreatToQuadrant:
                        manager.AddComponentData(entity,new StayInQuadrant(coolDownTime: 10f, spawnPosition: go.transform.position));
                        break;
                }
            }
            manager.AddBuffer<StateBuffer>(entity);

            manager.AddComponent<SetupBrainTag>(entity);


        }
        
    }
}