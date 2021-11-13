using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using IAUS.ECS.Component;
using Global.Component;
using Components.MovementSystem;
using DreamersInc.InflunceMapSystem;
using DreamersInc.FactionSystem;
public class BaseAIAuthoringSO : MonoBehaviour, IConvertGameObjectToEntity
{
    public AITarget Self;
    public Movement movement;
    public PatrolBuilderData buildPatrol;
    public bool AddPatrol;
    public bool AddWait;
    public bool AddRetreat;
    public WaitBuilderData waitBuilder;
    public RetreatCitizen retreatState;
    public AttackTargetState attackTargetState = new AttackTargetState();
    public List<AttackTypeInfo> GetAttackType;
    public InfluenceComponent GetInfluence;
    public int factionID; // ID Change
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, Self);
        dstManager.AddComponent<IAUSBrain>(entity);
        dstManager.SetComponentData(entity, new IAUSBrain { factionID = this.factionID });

        dstManager.AddComponent<SetupBrainTag>(entity);
        dstManager.AddComponentData(entity, GetInfluence);
        dstManager.AddBuffer<StateBuffer>(entity);
        if (AddPatrol)
        {
            Patrol patrol = new Patrol()
            {
                BufferZone = buildPatrol.BufferZone,
                _coolDownTime = buildPatrol.CoolDownTime
            };
            dstManager.AddComponentData(entity, patrol);
        }
        if (AddWait)
        {
            Wait  waitState = new Wait()
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
        if (AddRetreat) {
            dstManager.AddComponentData(entity, retreatState);
        }
        //if (GetAttackType.Count > 0)
        //{
        //    DynamicBuffer<AttackTypeInfo> ati = dstManager.AddBuffer<AttackTypeInfo>(entity);
        //    foreach (AttackTypeInfo Info in GetAttackType)
        //    {
        //        ati.Add(Info);
        //    }
        //    dstManager.AddComponentData(entity, attackTargetState);

        //}
    }
}
