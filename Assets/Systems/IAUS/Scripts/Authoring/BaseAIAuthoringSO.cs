using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using IAUS.ECS.Component;
using Global.Component;
using Components.MovementSystem;
using DreamersInc.InflunceMapSystem;
using AISenses.VisionSystems;
using DreamersInc.ComboSystem;

public class BaseAIAuthoringSO : MonoBehaviour, IConvertGameObjectToEntity
{
    public AITarget Self;
    public Movement movement;
    public PMovementBuilderData buildMovement;
    public bool AddPatrol;
    public bool AddTraverse;
    public bool AddWait;
    public bool AddRetreat;
    public WaitBuilderData waitBuilder;
    public RetreatCitizen retreatState;
    public AttackTargetState attackTargetState = new AttackTargetState();
    public List<AttackTypeInfo> GetAttackType;
    public InfluenceComponent GetInfluence;
    public int factionID;
    Entity entity;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        this.entity = entity;
    }

    public void SetupSystem()
    {
        
        EntityManager dstManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        dstManager.AddComponentData(entity, Self);

        dstManager.AddComponent<IAUSBrain>(entity);
        
        dstManager.SetComponentData(entity, new IAUSBrain
        {
            factionID = GetInfluence.factionID,
            Target = Self,
            NPCLevel = NPCLevel.Grunt, //  need to be added from EnemySO 
            Difficulty = Difficulty.Normal,
        });


        dstManager.AddComponent<SetupBrainTag>(entity);
        dstManager.AddComponentData(entity, GetInfluence);
        var Perception = new Perceptibility() { movement = MovementStates.Stadning_Still, noiseState = NoiseState.Normal, visibilityStates = VisibilityStates.Visible };
        dstManager.AddComponentData(entity, Perception);

        dstManager.AddBuffer<StateBuffer>(entity);
        if (AddPatrol)
        {
            Patrol patrol = new Patrol()
            {
                BufferZone = buildMovement.BufferZone,
                _coolDownTime = buildMovement.CoolDownTime
            };
            dstManager.AddComponentData(entity, patrol);
        }
        if (AddTraverse)
        {
            Traverse traverse = new Traverse()
            {
                BufferZone = buildMovement.BufferZone,
                _coolDownTime = buildMovement.CoolDownTime
            };
            dstManager.AddComponentData(entity, traverse);
        }
        if (AddWait)
        {
            Wait waitState = new Wait()
            {
                StartTime = waitBuilder.StartTime,
                _coolDownTime = waitBuilder.CoolDownTime
            };

            dstManager.AddComponentData(entity, waitState);

        }
        if (AddRetreat)
            dstManager.AddComponentData(entity, retreatState);
        if (Self.Type == TargetType.Character)
            dstManager.AddComponentData(entity, movement);
        if (AddRetreat)
        {
            dstManager.AddComponentData(entity, retreatState);
        }
        if (GetAttackType.Count != 0)
        {
            DynamicBuffer<AttackTypeInfo> ati = dstManager.AddBuffer<AttackTypeInfo>(entity);
            foreach (AttackTypeInfo Info in GetAttackType)
            {
                ati.Add(Info);
            }
            dstManager.AddComponentData(entity, attackTargetState);
            dstManager.AddBuffer<NPCAttackBuffer>(entity);
            dstManager.AddComponentData(entity, new Command() { InputQueue = new Queue<AnimationTrigger>() });


        }
    }

}
