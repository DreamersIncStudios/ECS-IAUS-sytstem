using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Stats
{
    public class CharacterSetup : MonoBehaviour
    {
    public CharacterClass CharClass;

        private void Awake()
        {
            if (CharClass.LevelMod == 0)
                CharClass.LevelMod = 1;

            if (CharClass.difficultyMod == 0)
                CharClass.difficultyMod = 1;

        }

        public void StatsUpdate(BaseCharacter CharacterStats)
        {
            CharacterStats.Level = CharClass.Level;
            float ModValue = CharClass.difficultyMod * CharClass.LevelMod * CharClass.Level;
            CharacterStats.GetPrimaryAttribute((int)AttributeName.Strength).BaseValue = (int)(CharClass.Strength * ModValue);
            CharacterStats.GetPrimaryAttribute((int)AttributeName.Awareness).BaseValue = (int)(CharClass.Awareness * ModValue);
            CharacterStats.GetPrimaryAttribute((int)AttributeName.Charisma).BaseValue = (int)(CharClass.Charisma * ModValue);
            CharacterStats.GetPrimaryAttribute((int)AttributeName.Resistance).BaseValue = (int)(CharClass.Resistance * ModValue);
            CharacterStats.GetPrimaryAttribute((int)AttributeName.WillPower).BaseValue = (int)(CharClass.WillPower * ModValue);
            CharacterStats.GetPrimaryAttribute((int)AttributeName.Vitality).BaseValue = (int)(CharClass.Vitality * ModValue);
            CharacterStats.GetPrimaryAttribute((int)AttributeName.Skill).BaseValue = (int)(CharClass.Skill * ModValue);
            CharacterStats.GetPrimaryAttribute((int)AttributeName.Speed).BaseValue = (int)(CharClass.Speed * ModValue);
            CharacterStats.GetPrimaryAttribute((int)AttributeName.Luck).BaseValue = (int)(CharClass.Luck * ModValue);
            CharacterStats.GetPrimaryAttribute((int)AttributeName.Concentration).BaseValue = (int)(CharClass.Concentration * ModValue);
            CharacterStats.GetVital((int)VitalName.Health).BaseValue = 50;
            CharacterStats.GetVital((int)VitalName.Mana).BaseValue = 25;

            CharacterStats.StatUpdate();

        }


    }
    [System.Serializable]
    public struct CharacterClass
    {
        public ClassTitle title;
        public int Level;
        public int Strength;
        public int Vitality;
        public int Awareness;
        public int Speed;
        public int Skill;
        public int Resistance;
        public int Concentration;
        public int WillPower;
        public int Charisma;
        public int Luck;

        public float difficultyMod;
        public float LevelMod;

    }
    public enum ClassTitle
    {
        Grunt, Soldier, Ranger, Archer, Sorcer, Mage, Monk, Swordman, Thief, Knight, Bot, Generalist, Pugiblist
    }
}