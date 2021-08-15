using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using IAUS.ECS2.Component;
using Global.Component;
using Components.MovementSystem;
using DreamersInc.InflunceMapSystem;

public class BaseAIAuthoringSO : MonoBehaviour, IConvertGameObjectToEntity
{
    public AITarget Self;
    public Movement movement;
    public Patrol patrolState;
    public bool AddPatrol;
    public bool AddWait;
    public bool AddRetreat;
    public Wait waitState;
    public RetreatCitizen retreatState;
    public AttackTargetState attackTargetState = new AttackTargetState();
    public List<AttackTypeInfo> GetAttackType;
    public InfluenceComponent GetInfluence;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, Self);
        dstManager.AddComponent<IAUSBrain>(entity);
        dstManager.AddComponent<SetupBrainTag>(entity);
        dstManager.AddComponentData(entity, GetInfluence);
        dstManager.AddBuffer<StateBuffer>(entity);
        if (AddPatrol)
            dstManager.AddComponentData(entity, patrolState);
        if (AddWait)
            dstManager.AddComponentData(entity, waitState);
        if (AddRetreat)
            dstManager.AddComponentData(entity, retreatState);
        if (Self.Type == TargetType.Character)
            dstManager.AddComponentData(entity, movement);
        if (AddRetreat) {
            dstManager.AddComponentData(entity, retreatState);
        }
        //if (GetAttackType.Count >= 1)
        //{
        //    DynamicBuffer<AttackTypeInfo> ATI = dstManager.AddBuffer<AttackTypeInfo>(entity);
        //    dstManager.AddComponentData(entity, attackTargetState);
        //    foreach (AttackTypeInfo Info in GetAttackType)
        //    {
        //        ATI.Add(Info);
        //    }
        //}
    }
}
