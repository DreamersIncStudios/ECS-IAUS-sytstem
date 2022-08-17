using DreamersInc.DamageSystem.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using DreamersInc.CombatSystem.Animation;
using System;
using System.Threading.Tasks;
using Random = UnityEngine.Random;

namespace Stats
{
    public class NPCChararacter : BaseCharacter,IConvertGameObjectToEntity
    {
        public override void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            float ModValue = 1.1f;
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

            base.Convert(entity, dstManager, conversionSystem);
            var data = new NPCStats()
            {
                MaxHealth = MaxHealth,
                MaxMana = MaxMana,
                CurHealth = CurHealth,
                CurMana = CurMana,
                selfEntityRef = entity
            };
            dstManager.AddComponentData(entity, data);
            Level = 5;
            StatUpdate();
        }
        public async void SetupNPCData(Entity entity, uint level)
        {

            float ModValue = (float)level * 1.5f;
            SelfEntityRef = entity;

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
            World.DefaultGameObjectInjectionWorld.EntityManager.SetComponentData(entity, new EnemyStats { selfEntityRef = entity });
            StatUpdate();
        }

        public override void TakeDamage(int Amount, TypeOfDamage typeOf, Element element)
        {
            //Todo Figure out element resistances, conditional mods, and possible affinity 
            float defense = typeOf switch
            {
                TypeOfDamage.MagicAoE => MagicDef,
                TypeOfDamage.Melee => MeleeDef,
                _ => MeleeDef,
            };
            Debug.Log((float)Amount / defense);
            int damageToProcess = -Mathf.FloorToInt(Amount * defense * Random.Range(.92f, 1.08f));
            AdjustHealth health = new AdjustHealth() { Value = damageToProcess };
            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(SelfEntityRef, health);
        }
        public override void ReactToHit(float impact, Vector3 Test, Vector3 Forward, TypeOfDamage typeOf = TypeOfDamage.Melee , Element element= Element.None)
        {
            //Todo Figure out element resistances, conditional mods, and possible affinity 
            float defense = typeOf switch
            {
                TypeOfDamage.MagicAoE => MagicDef,
                TypeOfDamage.Melee => MeleeDef,
                _ => MeleeDef,
            };

            ReactToContact reactTo = new ReactToContact() { 
                ForwardVector = Forward,
                positionVector = this.transform.position,
                RightVector = transform.right,
                HitIntensity = Mathf.FloorToInt( impact / (defense * 10.0f) * Random.Range(.92f, 1.08f)),
                HitContactPoint =Test
            };
            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(SelfEntityRef, reactTo);

        }
    }
}
