using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using AISenses;

public class SoundAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public SoundEmitter Emitter;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, Emitter);
      //  dstManager.SetName(entity, "Explosion");
    }

}
