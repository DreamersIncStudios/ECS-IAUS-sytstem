using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS2;
using ProjectRebirth.Bestiary.Interfaces;
using Components.MovementSystem;
using IAUS.ECS2.Character;
using Test.CharacterStats;
using InfluenceMap.Factions;

namespace IAUS.SpawnerSystem
{

    public class IAUSAuthoring : MonoBehaviour,IConvertGameObjectToEntity
    {
        [HideInInspector]public Entity Self { get { return selfRef; } }
        Entity selfRef;
        public ActiveAIStates StatesToAdd;
        public Patrol PatrolState;
        public Movement Move;
        public WaitTime Wait;
        public FollowCharacter Follow;
        public List<PatrolBuffer> Waypoints;
        [Range(0, 999)]
        public int CurHealth;
        [Range(0, 999)]
        public int CurMana;
        [Range(0, 999)]
        public int MaxHealth;
        [Range(0, 999)]
        public int MaxMana;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            selfRef = entity;
            dstManager.AddComponentData(entity, Move);
            dstManager.AddComponent<EnemyCharacter>(entity);
            dstManager.AddComponent<Unity.Transforms.CopyTransformFromGameObject>(entity);
            var data = new Stats() { CurHealth = CurHealth, CurMana = CurMana, MaxHealth = MaxHealth, MaxMana = MaxMana };
            dstManager.AddComponentData(entity, data);
            dstManager.AddComponent<NPC>(entity);

            if (StatesToAdd.Patrol)
            {
                dstManager.AddComponentData(entity, PatrolState);

                DynamicBuffer<PatrolBuffer> buffer = dstManager.AddBuffer<PatrolBuffer>(entity);
                foreach (var item in Waypoints)
                {
                    buffer.Add(item);
                }
                // add for loop adding patrol points;
            }
            if (StatesToAdd.Follow) {
                dstManager.AddComponentData(entity, Follow);
            }

            if (StatesToAdd.Wait)
            { dstManager.AddComponentData(entity,Wait); }

            dstManager.AddComponent<Attackable>(entity);
        }

        // Start is called before the first frame update
        void Start()
        {

        }



    }
}