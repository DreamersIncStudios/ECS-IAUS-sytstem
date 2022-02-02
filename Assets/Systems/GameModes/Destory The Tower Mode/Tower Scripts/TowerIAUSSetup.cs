using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Stats;
using IAUS.ECS.Component;
using Global.Component;
using AISenses.Authoring;
using DreamersInc.InflunceMapSystem;

namespace GameModes.DestroyTheTower.TowerSystem
{
    [RequireComponent(typeof(AISensesAuthoring))]
    public class TowerIAUSSetup : MonoBehaviour,IConvertGameObjectToEntity
    {
        public AITarget Self;
        public TowerData Data;
        public InfluenceComponent Influence;
        public List<AttackTypeInfo> GetAttackType;
        public AttackTargetState attackTargetState = new AttackTargetState();

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, Self);
            Influence.factionID = Self.FactionID;
            dstManager.AddComponentData(entity, Influence);
            var brain = new IAUSBrain() { factionID = Self.FactionID, Difficulty = Difficulty.Normal, Attitude = Status.Normal , NPCLevel = NPCLevel.Tower};
            dstManager.AddComponentData(entity,brain);
            dstManager.AddComponent<GatherResourcesState>(entity);
            dstManager.AddComponent<AttackTargetState>(entity);
            dstManager.AddComponent<SetupBrainTag>(entity); //TODO delay adding component;
            Data.EnergyLevel = 100;
            dstManager.AddComponentData(entity, Data);
            dstManager.AddBuffer<StateBuffer>(entity);
            dstManager.AddComponent<RepairState>(entity);
            dstManager.AddComponent<SpawnDefendersState>(entity);
            if (GetAttackType.Count != 0)
            {
                DynamicBuffer<AttackTypeInfo> ati = dstManager.AddBuffer<AttackTypeInfo>(entity);
                foreach (AttackTypeInfo Info in GetAttackType)
                {
                    ati.Add(Info);
                }
                dstManager.AddComponentData(entity, attackTargetState);

            }

        }


    }
}