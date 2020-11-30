using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Components.MovementSystem;
using IAUS.ECS2;

public class CitizenNPCAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{

    [HideInInspector] public Entity Self { get { return selfRef; } }
    Entity selfRef;
    public Patrol PatrolState = new Patrol()
    {
        Health = fullHealth,
        DistanceToTarget = distanceToPatrolTarget,
        BufferZone = .75f,
        ResetTimer = 4.5f,
        Status = ActionStatus.Idle,
   //     UpdatePostition = true,
        CanPatrol = true,
   //     LeaderUpdate = true


    };
    private static ConsiderationData fullHealth = new ConsiderationData
    {
        responseType = ResponseType.Logistic,
        M = 50,
        K = -1,
        B = .91f,
        C = .35f
    };
    private
        static ConsiderationData distanceToPatrolTarget = new ConsiderationData()
{
    responseType = ResponseType.Logistic,
    M = 50,
    K = -.95f,
    B = .935f,
    C = .35f
};

    public Movement Move;
    public WaitTime Wait;
    public List<PatrolBuffer> Waypoints;



    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        selfRef = entity;
        dstManager.AddComponentData(entity, Move);
        dstManager.AddComponent<Unity.Transforms.CopyTransformFromGameObject>(entity);

        dstManager.AddComponentData(entity, PatrolState);
        DynamicBuffer<PatrolBuffer> buffer = dstManager.AddBuffer<PatrolBuffer>(entity);
        foreach (var item in Waypoints)
        {
            buffer.Add(item);
        }

        dstManager.AddComponentData(entity, Wait);

    }
}
