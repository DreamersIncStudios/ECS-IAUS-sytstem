using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class Grunt : BaseAI
{
    public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        base.Convert(entity, dstManager, conversionSystem);
        //Add Additional Components Here;

    }

}
