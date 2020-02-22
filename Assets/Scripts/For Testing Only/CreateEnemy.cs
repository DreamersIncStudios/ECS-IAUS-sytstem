using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
namespace InfluenceMap
{
    public class CreateEnemy : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new EnemyCharacter() {
                Target = new Entity(),
                HaveTarget=false,
                gridpoint = new Gridpoint()
            });
            dstManager.AddComponentData(entity, new LookForPlayer());

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
}