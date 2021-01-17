using Unity.Entities;
using UnityEngine;
using IAUS.ECS2.Component;
using Global.Component;
using Components.MovementSystem;

public class BaseAIAuthoring : MonoBehaviour,IConvertGameObjectToEntity
{
    public AITarget Self;
    public Movement movement;
    public Patrol patrolState;
    public bool AddPatrol;
    public bool AddWait;
    public bool AddRetreat;
    public Wait waitState;
    public Retreat retreatState;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity,Self);
        dstManager.AddComponent<IAUSBrain>(entity);
        dstManager.AddComponent<SetupBrainTag>(entity);
        dstManager.AddBuffer<StateBuffer>(entity);
        if (AddPatrol)
            dstManager.AddComponentData(entity, patrolState);
        if(AddWait)
            dstManager.AddComponentData(entity, waitState);
        if(AddRetreat)
            dstManager.AddComponentData(entity, retreatState);
        if(Self.Type== TargetType.Character)
            dstManager.AddComponentData(entity, movement);


    }

}
