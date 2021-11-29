using UnityEngine;
using System.Collections;
using System;
using Unity.Entities;
using System.Threading.Tasks;
using DreamersInc.DamageSystem.Interfaces;
namespace Stats
{
    [Serializable]
    public abstract class BaseCharacter : MonoBehaviour, IConvertGameObjectToEntity, IDamageable
    {

        private string _name;
        private int _level;
        private uint _freeExp;
        private Attributes[] _primaryAttribute;
        private Vital[] _vital;
        private Stat[] _stats;
        private Abilities[] _ability;
        private Elemental[] _ElementalMods;

        public bool InvincibleMode;
        public bool Alive
        {
            get
            {
                bool temp = true;
                if (!InvincibleMode)
                {
                    temp = CurHealth > 0.0f;
                }
                return temp;
            }
        }

        [Range(0, 999)]
        public int CurHealth;

        [Range(0, 999)]
        [SerializeField] int maxHealth;
        public int MaxHealth { get { return MaxHealthMod + maxHealth; } set { maxHealth = value; } }
        public int MaxHealthMod { get; set; }
        [Range(0, 999)]
        public int CurMana;
        [Range(0, 999)]
        [SerializeField] int maxMana;
        public int MaxMana { get { return maxMana + MaxHealthMod; } set { maxMana = value; } }
        public int MaxManaMod { get; set; }

        public float MagicDef { get { return 1.0f / (float)(1.0f + ((float)GetStat((int)StatName.Magic_Defence).AdjustBaseValue / 100.0f)); } }
        public float MeleeDef { get { return 1.0f / (float)(1.0f + ((float)GetStat((int)StatName.Melee_Defence).AdjustBaseValue / 100.0f)); } }


        public bool Dead { get; private set; }

        public Entity SelfEntityRef { get; private set; }



        public void Awake()
        {
            _name = string.Empty;
            _level = 0;
            _freeExp = 0;
            _primaryAttribute = new Attributes[Enum.GetValues(typeof(AttributeName)).Length];
            _vital = new Vital[Enum.GetValues(typeof(VitalName)).Length];
            _stats = new Stat[Enum.GetValues(typeof(StatName)).Length];
            _ability = new Abilities[Enum.GetValues(typeof(AbilityName)).Length];
            // _ElementalMods = new Elemental[Enum.GetValues(typeof(Elements)).Length];
            SetupPrimaryAttributes();
            SetupVitals();
            SetupStats();
            SetupAbilities();

            CurHealth = MaxHealth;
            CurMana = MaxMana;
#if !UNITY_EDITOR
            InvincibleMode = false;
#endif

#if UNITY_EDITOR
            if (InvincibleMode)
                Debug.LogWarning("This Character is in Invincible Mode and will not take Damage", this);
#endif

            // SetupElementalMods();
        }



        public DynamicBuffer<EffectStatusBuffer> StatusBuffers;
        public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            SelfEntityRef = entity;
            dstManager.AddComponent<Unity.Transforms.CopyTransformFromGameObject>(entity);
            StatusBuffers = dstManager.AddBuffer<EffectStatusBuffer>(entity);

        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Level
        {
            get { return _level; }
            set { _level = value; }
        }

        public uint FreeExp
        {
            get { return _freeExp; }
            set { _freeExp = value; }
        }


        public void AddExp(uint exp)
        {
            _freeExp += exp;
            CalculateLevel();
        }

        public void CalculateLevel()
        {

            // TODO need to add logic here

        }

        private void SetupPrimaryAttributes()
        {
            for (int cnt = 0; cnt < _primaryAttribute.Length; cnt++)
            {
                _primaryAttribute[cnt] = new Attributes();
            }
        }

        public Attributes GetPrimaryAttribute(int index)
        {
            return _primaryAttribute[index];
        }

        public Elemental GetElementalMod(int index)
        {
            return _ElementalMods[index];

        }
        public void SetupElementalMods()
        {
            for (int cnt = 0; cnt < _ElementalMods.Length; cnt++)
            {
                _ElementalMods[cnt] = new Elemental();

            }
        }

        private void SetupVitals()
        {
            for (int cnt = 0; cnt < _vital.Length; cnt++)
                _vital[cnt] = new Vital();
            SetupVitalModifiers();
        }
        private void SetupAbilities()
        {
            for (int cnt = 0; cnt < _ability.Length; cnt++)
                _ability[cnt] = new Abilities();
            SetupAbilitesModifiers();
        }

        public Vital GetVital(int index)
        {
            return _vital[index];
        }
        public Abilities GetAbility(int index)
        {
            return _ability[index];
        }

        private void SetupStats()
        {
            for (int cnt = 0; cnt < _stats.Length; cnt++)
                _stats[cnt] = new Stat();
            SetupStatsModifiers();
        }

        public Stat GetStat(int index)
        {
            return _stats[index];
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
            await Task.Delay(TimeSpan.FromSeconds(1));
            StatUpdate();
        }


        public void StatUpdate()
        {
            for (int i = 0; i < _vital.Length; i++)
                _vital[i].Update();
            for (int j = 0; j < _stats.Length; j++)
                _stats[j].Update();
            for (int i = 0; i < _ability.Length; i++)
                _ability[i].Update();

            CurHealth = MaxHealth = GetVital((int)VitalName.Health).AdjustBaseValue;
            CurMana = MaxMana = GetVital((int)VitalName.Mana).AdjustBaseValue;
            World.DefaultGameObjectInjectionWorld.EntityManager.AddComponentData(SelfEntityRef, new LevelUpComponent() { MaxHealth = maxHealth, MaxMana = maxMana, CurHealth = CurHealth, CurMana = CurMana });
        }




        public void OnDeath(float deathDelay)
        {
            if (!Dead)
            {
                gameObject.SetActive(false);
                Debug.Log(Name + " is dead");
                //Destroy(this.gameObject, deathDelay);
                Dead = true;
            }
        }

        public void AddStatus(StatusEffect StatusToAdd) { }
        public void RemoveStatus(StatusEffect StatusToAdd) { }
        public void RemoveStatus(EffectStatus StatusName) { }

        public async void SetAttributes(int[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                _primaryAttribute[i].BaseValue = data[i];
            }
            await Task.Delay(TimeSpan.FromSeconds(1));
            StatUpdate();
        }



        public abstract void TakeDamage(int Amount, TypeOfDamage typeOf, Element element);

    }


}
