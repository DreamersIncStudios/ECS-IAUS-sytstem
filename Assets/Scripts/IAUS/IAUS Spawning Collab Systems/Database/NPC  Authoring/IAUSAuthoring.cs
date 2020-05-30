using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS2;
using ProjectRebirth.Bestiary.Interfaces;
using Components.MovementSystem;

namespace IAUS.SpawnerSystem
{

    public class IAUSAuthoring : MonoBehaviour,IConvertGameObjectToEntity
    {
        [HideInInspector]public Entity Self { get { return selfRef; } }
        Entity selfRef;
        public ActiveAIStates StatesToAdd;
        public Patrol PatrolState;
        public Movement Move;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            selfRef = entity;
            if (StatesToAdd.Patrol)
            {
                dstManager.AddComponentData(entity, PatrolState);
                dstManager.AddBuffer<PatrolBuffer>(entity);
                // add for loop adding patrol points;
            }
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