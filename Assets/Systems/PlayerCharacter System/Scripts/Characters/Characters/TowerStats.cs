using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Stats;
using System.Threading.Tasks;
using System;
using UnityEngine.Events;

namespace GameModes.DestroyTheTower.TowerSystem
{
    public class TowerStats : EnemyCharacter
    {
        public List<GameObject> Defenders;
        public UnityEvent Death;
        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            float ModValue = 1.1f;

            //base.Convert(entity, dstManager, conversionSystem);
            //var data = new EnemyStats() { MaxHealth = MaxHealth, MaxMana = MaxMana, CurHealth = CurHealth, CurMana = CurMana,
            //selfEntityRef = entity };
            //dstManager.AddComponentData(entity, data);

            GetPrimaryAttribute((int)AttributeName.Strength).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Awareness).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Charisma).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Resistance).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.WillPower).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Vitality).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Skill).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Speed).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Luck).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Concentration).BaseValue = (int)(20 * ModValue);
            GetVital((int)VitalName.Health).BaseValue = 50;
            GetVital((int)VitalName.Mana).BaseValue = 25;
         //   StatUpdate();
        }

        public async void UpdateLevel(int level)
        {
            float ModValue = level * 1.1f;

            GetPrimaryAttribute((int)AttributeName.Strength).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Awareness).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Charisma).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Resistance).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.WillPower).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Vitality).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Skill).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Speed).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Luck).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Concentration).BaseValue = (int)(20 * ModValue);
            GetVital((int)VitalName.Health).BaseValue = 50;
            GetVital((int)VitalName.Mana).BaseValue = 25;
            await Task.Delay(TimeSpan.FromSeconds(2));

            StatUpdate();

        }

        public async void  setupTower(uint level, Entity setEntity)
        {
            float ModValue = (float)level * 1.5f;
            SelfEntityRef = setEntity;
   
            GetPrimaryAttribute((int)AttributeName.Strength).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Awareness).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Charisma).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Resistance).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.WillPower).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Vitality).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Skill).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Speed).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Luck).BaseValue = (int)(20 * ModValue);
            GetPrimaryAttribute((int)AttributeName.Concentration).BaseValue = (int)(20 * ModValue);
            GetVital((int)VitalName.Health).StartValue = 500;
            GetVital((int)VitalName.Mana).StartValue = 250;
            await Task.Delay(TimeSpan.FromSeconds(2));
            World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(setEntity, new EnemyStats { selfEntityRef = setEntity });
            StatUpdate();

        }

        public void SpawnDefenders(int Level) {

        }
        bool quitting = false;
        
        private void OnApplicationQuit()
        {
            quitting = true;
        }

   
        private void OnDestroy()
        {
            if(!quitting && Death !=null)
                     Death.Invoke();
        }


    }
    [Serializable]
    public struct TowerData : IComponentData
    {
        public int level;
        [Tooltip("Rate in per second intervals")]
       [SerializeField] private float RepairRate; // TODO  Blob Asset 
        public float RepairRateFixed => RepairRate / 60;
        [Tooltip("Rate in per second intervals")]
        public float EnergyRecoverRate;
        [Tooltip("Rate in per second intervals")]
        [SerializeField] private float GatherResourcesRate;
        public float GatherResourcesRateFixed => GatherResourcesRate / 60;

        [SerializeField] public float ResourcesGathered { get; set; }
        [SerializeField] public float EnergyLevel { get; set; }
        public int MaxEnergy;
        public float EnergyRatio => EnergyLevel / (float)MaxEnergy;

        public void AdjustEnergy(float adj)
        {
            EnergyLevel += adj;
            if (EnergyLevel < 0)
            {
                EnergyLevel = 0;
            }
            if (EnergyLevel > MaxEnergy) { EnergyLevel = MaxEnergy; }
        }
    }
}