using System.Collections.Generic;

namespace Stats
{
    [System.Serializable]
    public class BaseStat
    {
       // private string _name;
        private int _baseValue;
        private int _buffValue;
        private int _expToLevel;
        private float _levelModifier;
   
        public BaseStat()
        {
            //_name = null;
            _baseValue = 0;
            _buffValue = 0;
            _levelModifier = 1.1f;
            _expToLevel = 100;
        }


        #region setters and getters

        public int BaseValue
        {
            get { return _baseValue; }
            set { _baseValue = value; }
        }
        public int BuffValue
        {
            get { return _buffValue; }
            set { _buffValue = value; }
        }
        public int ExpToLevel
        {
            get { return _expToLevel; }
            set { _expToLevel = value; }
        }
        public float LevelModifier
        {
            get { return _levelModifier; }
            set { _levelModifier = value; }
        }
        #endregion



        private int CalculateExpToLevel()
        {
            return (int)(_expToLevel * _levelModifier*_baseValue);
        }
 

        public void LevelUp()
        {
            _expToLevel = CalculateExpToLevel();
            _baseValue++;
        }

        public int AdjustBaseValue
        {
            get { return _baseValue + _buffValue; }
        }
  
    }
   
}

