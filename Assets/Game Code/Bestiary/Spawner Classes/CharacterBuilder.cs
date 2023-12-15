using System;
using System.Collections;
using System.Collections.Generic;
using AISenses;
using AISenses.VisionSystems.Combat;
using Components.MovementSystem;
using Dreamers.InventorySystem;
using Dreamers.InventorySystem.Base;
using DreamersInc.BestiarySystem;
using DreamersInc.ComboSystem;
using DreamersInc.InflunceMapSystem;
using Global.Component;
using IAUS.ECS;
using IAUS.ECS.Component;
using MotionSystem;
using MotionSystem.Components;
using Stats;
using Stats.Entities;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public class CharacterBuilder
{
    private GameObject model;
    private static Entity entity;
    private BaseCharacterComponent character;
    private int factionID;
    private uint classLevel;
    private string tag;
    public CharacterBuilder WithModel(GameObject go, Vector3 Position, string tagging)
    {
        model = Object.Instantiate(go);
        go.transform.position = Position;
        tag = tagging;
        tag = go.tag = tagging;
        return this;
    }

    public CharacterBuilder WithAnimation()
    {
        return this;
    }

    public CharacterBuilder WithAIControl()
    {
        return this;
    }

    public CharacterBuilder WithPlayerControl()
    {
        return this;
    }

    public CharacterBuilder WithVFX()
    {
        return this;
    }

    public CharacterBuilder WithEntityPhysics(PhysicsInfo physicsInfo)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        if (entity == Entity.Null)
        {
            Debug.Log("not entity");
            return this;
        }

        if (model == null)
        {
            Debug.Log("no model");
            return this;
        }
          var shape = new PhysicsShape();
            if (model.TryGetComponent<UnityEngine.CapsuleCollider>(out var capsule))
            {
                shape = PhysicsShape.Capsule;
            }
            if (model.TryGetComponent<UnityEngine.BoxCollider>(out var box))
            {
                shape = PhysicsShape.Box;
            }
         
            var spCollider = new BlobAssetReference<Unity.Physics.Collider>();
            switch (shape)
            {
                case PhysicsShape.Capsule:
                spCollider = Unity.Physics.CapsuleCollider.Create(new CapsuleGeometry()
                    {
                        Radius = capsule.radius,
                        Vertex0 = capsule.center + new Vector3(0, capsule.height, 0),
                        Vertex1 = new float3(0, 0, 0)

                    }, new CollisionFilter()
                    {
                        BelongsTo = physicsInfo.BelongsTo.Value,
                        CollidesWith = physicsInfo.CollidesWith.Value,
                        GroupIndex = 0
                    });


                    break;
                case PhysicsShape.Box:
                    if (box != null)
                        spCollider = Unity.Physics.BoxCollider.Create(new BoxGeometry()
                        {
                            Center = box.center,
                            Size = box.size,
                            Orientation = quaternion.identity,
                        }, new CollisionFilter()
                        {
                            BelongsTo = physicsInfo.BelongsTo.Value,
                            CollidesWith = physicsInfo.CollidesWith.Value,
                            GroupIndex = 0
                        });
                    manager.AddComponentData(entity, new PhysicsCollider()
                    { Value = spCollider });
                    break;
                case PhysicsShape.Sphere:
                    break;
                case PhysicsShape.Cyclinder:
                    break;
                case PhysicsShape.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            manager.AddSharedComponent(entity, new PhysicsWorldIndex());
            manager.AddComponentData(entity, new PhysicsCollider()
            { Value = spCollider });
            manager.AddComponentData(entity, new PhysicsInfo
            {
                BelongsTo = physicsInfo.BelongsTo,
                CollidesWith = physicsInfo.CollidesWith
            });
            var RB = model.GetComponent<Rigidbody>();
            manager.AddComponentObject(entity, RB);
        return this;
    }

    public CharacterBuilder WithStats(CharacterClass stats)
    {
        if (entity == Entity.Null) return this;
        if (model == null) return this;
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        BaseCharacterComponent character = new()
        {
            GOrepresentative = model // todo change to instance 
        };
        character.SetupDataEntity(stats);
        manager.AddComponentObject(entity, character);
        return this;
    }
    public CharacterBuilder WithCharacterDetection()
    {
        if (entity == Entity.Null) return this;
        if (model == null) return this;
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var vision = new Vision();
        vision.InitializeSense(character);
        manager.AddBuffer<ScanPositionBuffer>(entity);
        manager.AddComponentData(entity, vision);
        
        return this;
    }
    public CharacterBuilder WithInventorySystem(EquipmentSave Equipment)
    {
        if (entity == Entity.Null) return this;
        if (model == null) return this;
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        CharacterInventory inventory = new();
        inventory.Setup(Equipment, character);
        manager.AddComponentData(entity, inventory);
        
        return this;
    }
    public CharacterBuilder WithFactionInfluence(int factionID, int baseProtection, int baseThreat, uint classLevel)
    {
        this.factionID = factionID;
        this.classLevel = classLevel;
        if (entity == Entity.Null) return this;
        if (model == null) return this;
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        manager.AddComponentData(entity, new InfluenceComponent
        {
            factionID = factionID,
            Protection = baseProtection,
            Threat = baseThreat
        });
        manager.AddComponentData(entity, new AITarget()
        {
            FactionID = factionID,
            NumOfEntityTargetingMe = 3,
            CanBeTargetByPlayer = true,
            Type = TargetType.Character,
            level = classLevel,
            CenterOffset = new float3(0, 1, 0) //todo add value to SO
        }) ;
        
        manager.AddComponentData(entity, new Perceptibility
        {
            movement = MovementStates.Standing_Still,
            noiseState = NoiseState.Normal,
            visibilityStates = VisibilityStates.Visible
        });
        
        return this;
    }

    public CharacterBuilder WithMovement(MovementData Move)
    {        
        if (entity == Entity.Null) return this;
        if (model == null) return this;
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var agent = model.GetComponent<NavMeshAgent>();
        manager.AddComponentObject(entity, agent);
        var move = new Movement()
        {
            Acceleration = agent.acceleration,
            StoppingDistance = agent.stoppingDistance,
            Offset = agent.baseOffset,
        };
        move.SetMovementSpeed(character.GetPrimaryAttribute((int)AttributeName.Speed).AdjustBaseValue);
        manager.AddComponentData(entity, move);
        var controllerData = new CharControllerE();
        controllerData.Setup(Move, model.GetComponent<UnityEngine.CapsuleCollider>());
        manager.AddComponentData(entity, controllerData);

        return this;
    }

    public CharacterBuilder WithAI(NPCLevel getNpcLevel, List<AIStates> aiStatesToAdd, bool CapableOfMelee = false, bool CapableOfMagic =   false, bool CapableOfRange = false)
    {
        if (entity == Entity.Null) return this;
        if (model == null) return this;
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        manager.AddComponentData(entity, new IAUSBrain()
        {
            NPCLevel= getNpcLevel,
            FactionID = factionID
        });
        foreach (var state in aiStatesToAdd)
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
                        if (classLevel > 3)
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
                       
                        manager.AddComponentData(entity, new WanderQuadrant(
                            spawnPosition: model.transform.position,
                             coolDownTime: 5.5f,
                            bufferZone: .25f,
                            wanderNeighborQuadrants: false //TODO Figure out way above line causes issues
                        ));

                        break;
                    case AIStates.Wait:
                        var wait = new Wait()
                        {
                            _coolDownTime = 5.5f
                        };
                        manager.AddComponentData(entity, wait);
                        break;
                    case AIStates.Attack:
                        manager.AddComponent<AttackTarget>(entity);
                        manager.AddComponentObject(entity, new Command());
                        manager.AddComponentData(entity, new AttackState(5.5f, CapableOfMelee, CapableOfMagic, CapableOfRange));
                        manager.AddComponent<CheckAttackStatus>(entity);
                        manager.AddComponent<MagicAttackSubState>(entity);
                        manager.AddComponentData(entity, new RangedAttackSubState() { 
                            MaxEffectiveRange = 60,
                        });

                        manager.AddComponent<MagicMeleeAttackSubState>(entity);
                        var melee = new MeleeAttackSubState();
                        //if(info.AttackComboSO)
                         //   melee.SetupPossibleAttacks(info.AttackComboSO);
                        manager.AddComponentData(entity, melee);

                        break;
                    case AIStates.RetreatToLocation:
          
                        manager.AddComponentData(entity, new EscapeThreat(coolDownTime: 10f));
                        break;
                    case AIStates.RetreatToQuadrant:
                        manager.AddComponentData(entity,new StayInQuadrant(coolDownTime: 10f, spawnPosition: model.transform.position));
                        break;
                }
            }
            manager.AddBuffer<StateBuffer>(entity);

            manager.AddComponent<SetupBrainTag>(entity);

        return this;
    }
    
    public Entity Build()
    {
  
        return entity;
    }

    public static CharacterBuilder CreateCharacter(string entityName)
    {
        EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype baseEntityArch = manager.CreateArchetype(
            typeof(LocalTransform),
            typeof(LocalToWorld)
        );
        Entity baseDataEntity = manager.CreateEntity(baseEntityArch);
        if (entityName != string.Empty)
            manager.SetName(baseDataEntity, entityName);
        else
            manager.SetName(baseDataEntity, "NPC Data");
        manager.SetComponentData(baseDataEntity, new LocalTransform() { Scale = 1 });
        entity = baseDataEntity;

        return new CharacterBuilder();
    }
    
}
