using UnityEngine;
using Unity.Entities;
using System.Collections;

namespace InfluenceMap
{
    public class CoverAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Header("Cover Damage")]
        public Influencer influence;
        [Header("Cover Influence")]
        public Cover cover;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            dstManager.AddComponentData(entity, cover);
            dstManager.AddComponentData(entity, influence);
      
        }



        // Update is called once per frame
        void Update()
        {

        }
    }
}