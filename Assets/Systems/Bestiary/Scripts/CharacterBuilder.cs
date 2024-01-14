using System.Collections;
using System.Collections.Generic;
using Components.MovementSystem;
using IAUS;
using IAUS.Components.States;
using MotionSystem;
using MotionSystem.Components;
using Stats;
using Stats.Entities;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

public class CharacterBuilder 
{
    private GameObject model;
    private static Entity entity;
    private BaseCharacterComponent character;
    private int factionID;
    private uint classLevel;
    private string tag;
    
        
    public Entity Build()
    {
  
        return entity;
    }

    public CharacterBuilder WithModel(GameObject go, Vector3 Position, string tagging)
    {
        return WithModel(go,Position,tagging,out _);
    }  
    public CharacterBuilder WithModel(GameObject go, Vector3 Position, string tagging, out GameObject Spawned)
    {
        Spawned = model = Object.Instantiate(go);
        go.transform.position = Position;
        tag = tagging;
        tag = go.tag = tagging;
        return this;
    }
    public CharacterBuilder WithAnimation()
    {
        if (entity == Entity.Null) return this;
        if (model == null) return this;
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        TransformGO transformLink = new()
        {
            transform = model.transform
        };
        manager.AddComponentData(entity, transformLink);
        return this;
    }

    public CharacterBuilder WithAIMovement(MovementData MoveData)
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
        controllerData.Setup(MoveData, model.GetComponent<UnityEngine.CapsuleCollider>());
        manager.AddComponentData(entity, controllerData);
        return this;
    }

    public CharacterBuilder WithStats(CharacterClass stats)
    {
        if (entity == Entity.Null) return this;
        if (model == null) return this;
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;

        BaseCharacterComponent data = new()
        {
            GOrepresentative = model 
        };
        data.SetupDataEntity(stats);
        manager.AddComponentObject(entity, data);

        this.character = data;
        return this;
    }
    public CharacterBuilder WithAIControl(List<AIStates> aiStatesToAdd)
    {
        if (entity == Entity.Null) return this;
        if (model == null) return this;
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        manager.AddComponentData(entity, new AI_Control());
        manager.AddComponentData(entity, new AIStat() { Speed = 10 }); // TODO This need to pull from stat
        manager.AddComponentData(entity, new PatrolArea());
        return this;
    }

    public static CharacterBuilder CreateCharacter(string entityName, out Entity spawnedEntity)
    {
        var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var baseEntityArch = manager.CreateArchetype(
            typeof(LocalTransform),
            typeof(LocalToWorld)
        );
        var baseDataEntity = manager.CreateEntity(baseEntityArch);
        manager.SetName(baseDataEntity, entityName != string.Empty ? entityName : "NPC Data");
        manager.SetComponentData(baseDataEntity, new LocalTransform() { Scale = 1 });
        spawnedEntity =  entity = baseDataEntity;

        return new CharacterBuilder();
    }

    public static CharacterBuilder CreateCharacter(string entityName)
    {
        return CreateCharacter(entityName, out _);
    }
}
