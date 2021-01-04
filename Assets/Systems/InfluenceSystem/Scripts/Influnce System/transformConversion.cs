using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
public class transformConversion : MonoBehaviour,IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<Unity.Transforms.CopyTransformFromGameObject>(entity);

    }

}
