using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.Consideration;

public class MoveTo : MonoBehaviour,IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new MoveToComponent() { DS = new DistanceConsider()
        {
            responseType = ResponseType.Logistic,
            M = 50,
            K = .85f,
            B = 0.15f,
            C = .6f
        }, Target = new Vector3(20, 0, 35) };
        dstManager.AddComponentData(entity, data);
    }

}

public struct MoveToComponent : IComponentData {

    public DistanceConsider DS;
    public Vector3 Target;
}
