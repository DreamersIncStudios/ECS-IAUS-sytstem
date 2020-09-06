using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterAlignmentSystem;
using Unity.Entities;

namespace CharacterAlignmentSystem.Authoring
{
    public class AISensesAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public bool Vision;
        public Vision VisionData;
        public bool Hearing;
   
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
            dstManager.AddBuffer<ScanPositionBuffer>(entity);
            if (Vision) 
                dstManager.AddComponentData(entity, VisionData);

        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public enum SenseToEdit { 
            vison, hear, Touch
        }
    }
}