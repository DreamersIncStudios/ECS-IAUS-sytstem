using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Entities;
using System.Threading.Tasks;
using DreamersInc.DamageSystem.Interfaces;
namespace Stats
{
    public abstract partial class BaseCharacter : MonoBehaviour, IConvertGameObjectToEntity, IDamageable
    {

        private void SetupVitalBase()
        {
            //health
            GetVital((int)VitalName.Health).AddDefiningAttribute(
                new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Vitality), 3f)
            );
            GetVital((int)VitalName.Health).AddDefiningAttribute(
                new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Resistance), 3f)
                );
            GetVital((int)VitalName.Health).AddDefiningAttribute(
                new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Level), 10.0f)
                );
            GetVital((int)VitalName.Health).AddDefiningAttribute(
               new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Luck), .5f)
               );


            //energy
            GetVital((int)VitalName.Energy).AddDefiningAttribute(
                new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.WillPower), 1)
            );

            //mana
            GetVital((int)VitalName.Mana).AddDefiningAttribute(
                new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.WillPower), 2.5f)
            );
            GetVital((int)VitalName.Mana).AddDefiningAttribute(
                new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Concentration), 1.75f)
                );
        }


        private void SetupVitalModifiers()
        {
            //health
            GetVital((int)VitalName.Health).AddModifier(
                new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Vitality), 3f)
            );
            GetVital((int)VitalName.Health).AddModifier(
                new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Resistance), 3f)
                );
            GetVital((int)VitalName.Health).AddModifier(
                new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Level), 10.0f)
                );
            GetVital((int)VitalName.Health).AddModifier(
               new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Luck), .5f)
               );


            //energy
            GetVital((int)VitalName.Energy).AddModifier(
                new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.WillPower), 1)
            );

            //mana
            GetVital((int)VitalName.Mana).AddModifier(
                new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.WillPower), 2.5f)
            );
            GetVital((int)VitalName.Mana).AddModifier(
                new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Concentration), 1.75f)
                );
        }

        public void SetupStatsBase()
        {
            //Need to Update with Calculation based on FFXV and FFXIII
            GetStat((int)StatName.Melee_Offence).AddDefiningAttribute(
                new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Strength), 1.5f));
            GetStat((int)StatName.Melee_Offence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Skill), 1.250f));
            GetStat((int)StatName.Melee_Offence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Level), 3.0f));
            GetStat((int)StatName.Melee_Defence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Vitality), 1));


            GetStat((int)StatName.Magic_Offence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Concentration), .5f));
            GetStat((int)StatName.Magic_Offence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.WillPower), .5f));
            GetStat((int)StatName.Magic_Offence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Charisma), .5f));

            GetStat((int)StatName.Magic_Defence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Strength), .2f));
            GetStat((int)StatName.Magic_Defence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Charisma), .33f));
            GetStat((int)StatName.Magic_Defence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Speed), .10f));
            GetStat((int)StatName.Magic_Defence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Charisma), .45f));

            GetStat((int)StatName.Ranged_Offence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Concentration), .33f));
            GetStat((int)StatName.Ranged_Offence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Speed), .33f));

            GetStat((int)StatName.Ranged_Defence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Speed), .33f));
            GetStat((int)StatName.Ranged_Defence).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), .33f));

            //Targeting and Motion detection
            GetStat((int)StatName.Range_Motion).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), .33f));
            GetStat((int)StatName.Range_Target).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), .33f));
            // Status Changes IE Poison Confused Berzerk etc...

            GetStat((int)StatName.Status_Change).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), .33f));
            GetStat((int)StatName.Status_Change).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Resistance), .33f));
            // Recovery Rates for Mana;

            GetStat((int)StatName.Mana_Recover).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.WillPower), .25f));
            GetStat((int)StatName.Mana_Recover).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Concentration), .25f));
        }

        public void SetupStatsModifiers()
        {
            //Need to Update with Calculation based on FFXV and FFXIII
            GetStat((int)StatName.Melee_Offence).AddModifier(
                new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Strength), 1.5f));
            GetStat((int)StatName.Melee_Offence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Skill), 1.250f));
            GetStat((int)StatName.Melee_Offence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Level), 3.0f));
            GetStat((int)StatName.Melee_Defence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Vitality), 1));


            GetStat((int)StatName.Magic_Offence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Concentration), .5f));
            GetStat((int)StatName.Magic_Offence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.WillPower), .5f));
            GetStat((int)StatName.Magic_Offence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Charisma), .5f));

            GetStat((int)StatName.Magic_Defence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Strength), .2f));
            GetStat((int)StatName.Magic_Defence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Charisma), .33f));
            GetStat((int)StatName.Magic_Defence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Speed), .10f));
            GetStat((int)StatName.Magic_Defence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Charisma), .45f));

            GetStat((int)StatName.Ranged_Offence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Concentration), .33f));
            GetStat((int)StatName.Ranged_Offence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Speed), .33f));

            GetStat((int)StatName.Ranged_Defence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Speed), .33f));
            GetStat((int)StatName.Ranged_Defence).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), .33f));

            //Targeting and Motion detection
            GetStat((int)StatName.Range_Motion).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), .33f));
            GetStat((int)StatName.Range_Target).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), .33f));
            // Status Changes IE Poison Confused Berzerk etc...

            GetStat((int)StatName.Status_Change).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), .33f));
            GetStat((int)StatName.Status_Change).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Resistance), .33f));
            // Recovery Rates for Mana;

            GetStat((int)StatName.Mana_Recover).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.WillPower), .25f));
            GetStat((int)StatName.Mana_Recover).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Concentration), .25f));
        }

        public void SetupAbilitesBase()
        {
            GetAbility((int)AbilityName.Libra).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), .55f));
            GetAbility((int)AbilityName.Detection).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), .55f));

            GetAbility((int)AbilityName.Detection).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), 2.75f));
            GetAbility((int)AbilityName.Detection).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Skill), 2.75f));
            GetAbility((int)AbilityName.Detection).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Concentration), 2.75f));
            GetAbility((int)AbilityName.Detection).AddDefiningAttribute(new BaseDefiningAttribute(GetPrimaryAttribute((int)AttributeName.Luck), 2.15f));



        }


        public void SetupAbilitesModifiers()
        {
            GetAbility((int)AbilityName.Libra).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), .55f));
            GetAbility((int)AbilityName.Detection).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), .55f));

            GetAbility((int)AbilityName.Detection).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Awareness), 2.75f));
            GetAbility((int)AbilityName.Detection).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Skill), 2.75f));
            GetAbility((int)AbilityName.Detection).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Concentration), 2.75f));
            GetAbility((int)AbilityName.Detection).AddModifier(new ModifyingAttribute(GetPrimaryAttribute((int)AttributeName.Luck), 2.15f));



        }

        public async void SetAttributeBaseValue(int level, int BaseHealth, int BaseMana, int Str, int vit, int Awr, int Spd, int Skl, int Res, int Con, int Will, int Chars, int Lck)
        {
            _level = GetPrimaryAttribute((int)AttributeName.Level).BaseValue = level;
            GetPrimaryAttribute((int)AttributeName.Strength).BaseValue = Str;
            GetPrimaryAttribute((int)AttributeName.Vitality).BaseValue = vit;
            GetPrimaryAttribute((int)AttributeName.Awareness).BaseValue = Awr;
            GetPrimaryAttribute((int)AttributeName.Speed).BaseValue = Spd;
            GetPrimaryAttribute((int)AttributeName.Skill).BaseValue = Skl;
            GetPrimaryAttribute((int)AttributeName.Resistance).BaseValue = Res;
            GetPrimaryAttribute((int)AttributeName.Concentration).BaseValue = Con;
            GetPrimaryAttribute((int)AttributeName.WillPower).BaseValue = Will;
            GetPrimaryAttribute((int)AttributeName.Charisma).BaseValue = Chars;
            GetPrimaryAttribute((int)AttributeName.Luck).BaseValue = Lck;
            GetVital((int)VitalName.Health).BuffValue = BaseHealth;
            GetVital((int)VitalName.Mana).BuffValue = BaseMana;
            await Task.Delay(TimeSpan.FromSeconds(2));
            StatUpdate();
        }



    }
}
