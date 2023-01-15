using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace AISenses.Authoring
{
    public class AISensesAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public bool Vision;
        public Vision VisionData;

        // hearing component

        public bool Touch;
        // Touch component

         public SenseToEdit Editing;
        public Vector3 DirFromAngle(float angleInDegree, bool angleIsGlobal)
        {
            if (angleIsGlobal)
            { angleInDegree += transform.eulerAngles.y; }

            return new Vector3(Mathf.Sin(angleInDegree * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegree * Mathf.Deg2Rad));
        }
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent<Unity.Transforms.CopyTransformFromGameObject>(entity);

            dstManager.AddBuffer<ScanPositionBuffer>(entity);
            dstManager.AddComponent<AlertLevel>(entity);
            if (Vision) 
                dstManager.AddComponentData(entity, VisionData);
        }
        //Todo Make so vison system can only track one area at time for large  view area
        //Todo Add Sweeping scan mode. 

        public enum SenseToEdit { 
            vison, hear, Touch
        }
    }
}