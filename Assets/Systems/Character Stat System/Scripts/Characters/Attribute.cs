namespace Stats
{
    [System.Serializable]
    public class Attributes : BaseStat
    {
        public Attributes()
        {
            BaseValue = 0;
            ExpToLevel = 50;
            LevelModifier = 1.05f;
        }
    }

    public enum AttributeName
    {
        Level,
        Strength,
        Vitality,
        Awareness,
        Speed,
        Skill,
        Resistance,
        Concentration,
        WillPower,
        Charisma,
        Luck
    }
    [System.Serializable]
    public struct StatModifier
    {
        public AttributeName Stat;
        public int BuffValue;

    }
}