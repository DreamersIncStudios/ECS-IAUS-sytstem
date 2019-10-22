using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.Component;
using Unity.Mathematics;

public abstract class BaseAI : MonoBehaviour,IConvertGameObjectToEntity
{
    public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {
        var Move = new Movement() { CanMove = true, TargetLocation = new float3(10, 0, 10), MovementSpeed = 2, StoppingDistance = 1 };
        dstManager.AddComponentData(entity, Move);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
