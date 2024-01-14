namespace Stats
{
    [System.Serializable]
    public class Vital : ModifiedStat
    {
        private int _curValue;

        public Vital()
        {
            _curValue = 0;
            ExpToLevel = 100;
            LevelModifier = 1.1f;
        }

        public int CurValue
        {
            get
            {
                if (_curValue > AdjustBaseValue)
                    return AdjustBaseValue;
                return _curValue;
            }
            set { _curValue = value; }
        }
    }

    public enum VitalName
    {
        Health,
        Energy,
        Mana
    }
}