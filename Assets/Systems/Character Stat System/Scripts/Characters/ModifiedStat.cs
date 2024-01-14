using System.Collections.Generic;
namespace Stats
{
    [System.Serializable]
    public class ModifiedStat : BaseStat
    {
        private List<ModifyingAttribute> _mods;
        private List<BaseDefiningAttribute> _atts;
        private int _modValue;
        private int _start;
        public int StartValue
        {
            get { return _start; }
            set { _start = value; }
        }

        public ModifiedStat()
        {
            _mods = new List<ModifyingAttribute>();
            _atts = new List<BaseDefiningAttribute>();

            _modValue = 0;
        }

        public void AddModifier(ModifyingAttribute mod)
        {
            _mods.Add(mod);
        }
        public void AddDefiningAttribute(BaseDefiningAttribute att)
        {
            _atts.Add(att);
        }

        public void CalculateModValue()
        {
            _modValue = 0;
            if (_mods.Count > 0)
            {
                foreach (ModifyingAttribute mod in _mods)
                {
                    _modValue += (int)(mod.attribute.BuffValue * mod.ratio);
                }
            }
        }
        public void CalculateAttValue()
        {
            BaseValue = 0;
            if (_atts.Count > 0)
            {
                foreach (BaseDefiningAttribute att in _atts)
                {
                    BaseValue += (int)(att.attribute.BaseValue * att.ratio);
                }
            }
        }

        public new int AdjustBaseValue
        {
            get { return StartValue + BaseValue + BuffValue + _modValue; }
        }

        public void Update()
        {
            CalculateAttValue();

            CalculateModValue();
        }
    }

    public struct ModifyingAttribute
    {
        public Attributes attribute;
        public float ratio;

        public ModifyingAttribute(Attributes att, float rat)
        {
            attribute = att;
            ratio = rat;
        }
    }

    public struct BaseDefiningAttribute
    {

        public Attributes attribute;
        public float ratio;

        public BaseDefiningAttribute(Attributes att, float rat)
        {
            attribute = att;
            ratio = rat;
        }
    }
}
