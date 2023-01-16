using Stats;
using Stats.Entities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
namespace Stats.Entities
{
    public partial class BaseCharacterComponent : IComponentData
    {
        public void ModCharacterStats(List<StatModifier> Modifiers, bool Add)
        {
            int MP = 1;
            if (!Add)
            {
                MP = -1;
            }
            foreach (StatModifier mod in Modifiers)
            {

                switch (mod.Stat)
                {
                    case AttributeName.Level:
                        Debug.LogWarning("Level Modding is not allowed at this time. Please contact Programming is needed");
                        break;
                    case AttributeName.Strength:
                        GetPrimaryAttribute((int)AttributeName.Strength).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Vitality:
                        GetPrimaryAttribute((int)AttributeName.Vitality).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Awareness:
                        GetPrimaryAttribute((int)AttributeName.Awareness).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Speed:
                        GetPrimaryAttribute((int)AttributeName.Speed).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Skill:
                        GetPrimaryAttribute((int)AttributeName.Skill).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Resistance:
                        GetPrimaryAttribute((int)AttributeName.Resistance).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Concentration:
                        GetPrimaryAttribute((int)AttributeName.Concentration).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.WillPower:
                        GetPrimaryAttribute((int)AttributeName.WillPower).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Charisma:
                        GetPrimaryAttribute((int)AttributeName.Charisma).BuffValue += mod.BuffValue * MP;
                        break;
                    case AttributeName.Luck:
                        GetPrimaryAttribute((int)AttributeName.Luck).BuffValue += mod.BuffValue * MP;
                        break;
                }
            }
            StatUpdate();

        }
    }
}