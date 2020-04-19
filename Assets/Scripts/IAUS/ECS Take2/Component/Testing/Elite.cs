using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using CharacterStats;

namespace IAUS.ECS2.Charaacter
{
    public class Elite : CharacterAuthoring
    {
        public int MaxNumOfSubs;
        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            base.Convert(entity, dstManager, conversionSystem);
            var EliteComp = new EliteComponent() { MaxNumOfSubs = MaxNumOfSubs };
            dstManager.AddComponentData(entity,EliteComp);
        }

    }


    public struct EliteComponent : IComponentData {
        public int NumOfSubs;
        public int MaxNumOfSubs;
        public bool IsNotMaxed { get { return NumOfSubs < MaxNumOfSubs; } }
    }
}