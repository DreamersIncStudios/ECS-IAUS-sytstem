using DreamersInc.InfluenceMapSystem;
using Global.Component;
using Stats;
using Stats.Entities;
using DreamersIncStudio.FactionSystem;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


namespace DreamersInc.BestiarySystem.Testing
{
    public class TestingDummyInfo : ScriptableObject
    {
        [SerializeField] private uint creatureID;
        public uint ID { get { return creatureID; } }
        public string Name;
        public ICharacterData stats;
        public GameObject Prefab;
        public PhysicsInfo PhysicsInfo;

        [Header("influence ")]
        public FactionNames FactionID;
        [FormerlySerializedAs("BaseThreat")] public int InfluenceValue;
     
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
        private const string NPCFolderPath = "Assets/Prefab Library/Resources/Item Database/Spells";

        [MenuItem("Assets/Create/Bestiary/Test Dummy Info")]
        static public void CreateTestDummyInfo()
        {
            Dreamers.Global.ScriptableObjectUtility.CreateAsset<TestingDummyInfo>(NPCFolderPath,"Creature", out TestingDummyInfo info);
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
                go.layer = 6;
                EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
                entity = CreateEntity(manager, go.transform, info.Name + " NPC");
                AddPhysics(manager, entity, go, info.PhysicsInfo);
                BaseCharacterComponent character = new()
                {
                    GORepresentative = go
                };
                character.SetupDataEntity(info.stats,info.Name);
         ;
                manager.AddComponentObject(entity, go.transform);
                manager.AddComponentObject(entity, character);
                manager.AddComponentData(entity, 
                    new InfluenceComponent(info.FactionID, info.InfluenceValue,15)); //Todo add range of Influence to CharacterInfo Scriptable Object
                manager.AddComponentData(entity, new AITarget()
                {
                    FactionID = info.FactionID,
                    NumOfEntityTargetingMe = 3,
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