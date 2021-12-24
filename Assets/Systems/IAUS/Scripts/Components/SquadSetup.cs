using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace IAUS.ECS.Component
{
    public class SquadSetup : MonoBehaviour, IConvertGameObjectToEntity
    {
        public int SquadID;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var data = new SquadMembership();
            data.SetSquadID(SquadID);
            dstManager.AddComponentData(entity, data);
        }
    }
}