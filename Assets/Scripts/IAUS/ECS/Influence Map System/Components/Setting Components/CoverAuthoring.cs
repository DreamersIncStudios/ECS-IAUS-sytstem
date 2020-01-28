using UnityEngine;
using Unity.Entities;
using System.Collections;

namespace InfluenceMap
{
    public class CoverAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Header("Cover Damage")]
        public DestructionType Destruction;
        public int level;
        public int MaxHealth;
        public int Range;
        public int Damage;

        [Header("Cover Influence")]
        public float Influence;
        public Threat threat;
        public Threat Protection;
        public FallOff fallOff;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            var influence = new Influencer()
            {
                Range = Range,
                fallOff = fallOff,
                threat =threat,
                Protection=Protection
            };
            var cover = new Cover()
            {
                Damage = Damage,
                Range = Range,
                Destroyed = false,
                Destruction=Destruction,
                level=level,
                MaxHealth=MaxHealth,
                CurHealth=MaxHealth
            
            };

            dstManager.AddComponentData(entity, cover);
            dstManager.AddComponentData(entity, influence);

         
        }



        // Update is called once per frame
        void Update()
        {

        }
    }
}