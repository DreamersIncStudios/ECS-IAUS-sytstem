using DreamersInc.InflunceMapSystem;
using Global.Component;
using MotionSystem;
using Stats;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;



namespace DreamersInc.BestiarySystem.Testing
{
    public class TestingDummyInfo : ScriptableObject
    {
        [SerializeField] private uint creatureID;
        public uint ID { get { return creatureID; } }
        public string Name;
        public CharacterClass stats;
        public GameObject Prefab;
        public PhysicsInfo PhysicsInfo;

        [Header("influence ")]
        public int factionID;
        public int BaseThreat;
        public int BaseProtection;
#if UNITY_EDITOR

        public void setItemID(uint ID)
        {

            this.creatureID = ID;
        }
#endif
    }

#if UNITY_EDITOR
    public static partial class Creator
    {
        [MenuItem("Assets/Create/Test Dummy Info")]
        static public void CreateTestDummyInfo()
        {
            Dreamers.Global.ScriptableObjectUtility.CreateAsset<TestingDummyInfo>("Creature", out TestingDummyInfo info);
            BestiaryDB.LoadDatabase(true);
            info.setItemID((uint)BestiaryDB.Dummies.Count + 1);
        }

    }
#endif

}
namespace DreamersInc.BestiarySystem
{


    public sealed partial class BestiaryDB : MonoBehaviour
    {
        public static bool SpawnDummy(uint ID, out GameObject go, out Entity entity) {
            var info = GetDummy(ID);
                if (info != null) 
            {
                go = Instantiate(info.Prefab);
               // go.layer = 6;
                EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                entity = CreateEntity(manager, go.transform, info.Name + " NPC");
                AddPhysics(manager, entity, go, info.PhysicsInfo);
                BaseCharacterComponent character = new()
                {
                    GOrepresentative = go
                };
                character.SetupDataEntity(info.stats);
                TransformGO transformLink = new()
                {
                    transform = go.transform
                };
                manager.AddComponentData(entity, transformLink);
                manager.AddComponentObject(entity, character);
                manager.AddComponentData(entity, new InfluenceComponent
                {
                    factionID = info.factionID,
                    Protection = info.BaseProtection,
                    Threat = info.BaseThreat
                });
                manager.AddComponentData(entity, new AITarget()
                {
                    FactionID = info.factionID,
                    NumOfEntityTargetingMe = 300,
                    CanBeTargetByPlayer = true,
                    Type = TargetType.Character,
                    CenterOffset = new float3(0, 1, 0) //todo add value to SO
                });
            }
                else 
            {
                go = null;
                entity = Entity.Null;
            }
            return info != null;
        }

        public static bool SpawnDummy(uint ID, Vector3 Position)
        {
            if (SpawnDummy(ID, out GameObject go, out Entity _))
            {
                go.transform.position = Position;
                return true;
            }
            else { return false; }
        }

    }
}