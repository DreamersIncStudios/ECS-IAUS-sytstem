using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Stats;
using IAUS.ECS.Component;
using Global.Component;

namespace GameModes.DestroyTheTower.TowerSystem
{
    public class TowerIAUSSetup : MonoBehaviour,IConvertGameObjectToEntity
    {
        public AITarget Self;
        public TowerData Data;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, Self);
            var brain = new IAUSBrain() { factionID = Self.FactionID, Difficulty = Difficulty.Normal, Attitude = Status.Normal , NPCLevel = NPCLevel.Tower};
            dstManager.AddComponentData(entity,brain);
            dstManager.AddComponent<GatherResourcesState>(entity);
            dstManager.AddComponent<AttackTargetState>(entity);
            dstManager.AddComponent<SetupBrainTag>(entity); //TODO delay adding component;
            Data.EnergyLevel = 100;
            dstManager.AddComponentData(entity, Data);
            dstManager.AddBuffer<StateBuffer>(entity);
            //dstManager.AddComponent<RepairState>(entity);
            //dstManager.AddComponent<SpawnDefendersState>(entity);

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