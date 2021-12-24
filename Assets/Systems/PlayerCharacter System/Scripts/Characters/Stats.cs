namespace Stats
{[System.Serializable]
    public class Stat : ModifiedStat
    {
        private bool _known;

        public Stat()
        {
            _known = false;
            ExpToLevel = 25;
            LevelModifier = 1.1f;
        }
        public bool Known
        {
            get { return _known; }
            set { _known = value; }
        }
    }

    public enum StatName
    {
        Melee_Offence,
        Melee_Defence,
        Ranged_Offence,
        Ranged_Defence,
        Magic_Offence,
        Magic_Defence,
        Range_Target,
        Range_Motion,
        Status_Change,
        Mana_Recover
    }
}