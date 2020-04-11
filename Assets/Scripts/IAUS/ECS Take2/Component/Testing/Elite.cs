using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using CharacterStats;

namespace IAUS.ECS2.Charaacter
{
    public class Elite : CharacterAuthoring
    {
        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);

            dstManager.AddComponent<EliteComponent>(entity);
        }

    }


    public struct EliteComponent : IComponentData { }
}