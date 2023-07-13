    using DreamersInc.DamageSystem.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

[assembly: InternalsVisibleTo("PlayerCharacterSystem.Player")]
namespace Stats
{
    [Serializable]
    public  abstract partial class BaseCharacter : MonoBehaviour, IDamageable
    {

        private string _name;
        private int _level;
        private uint _freeExp;
        private Attributes[] _primaryAttribute;
        private Vital[] _vital;
        private Stat[] _stats;
        private Abilities[] _ability;
        private Elemental[] _ElementalMods;
        public bool InPlay;
        public bool InvincibleMode;

        public AlteredStatus AlteredStatus { get; private set; }

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

        [Range(0, 9999)]
        public int CurHealth;

        [Range(0, 9999)]
        [SerializeField] int maxHealth;
        public int MaxHealth { get { return MaxHealthMod + maxHealth; } set { maxHealth = value; } }
        public int MaxHealthMod { get; set; }
        [Range(0, 9999)]
        public int CurMana;
        [Range(0, 9999)]
        [SerializeField] int maxMana;
        public int MaxMana { get { return maxMana + MaxHealthMod; } set { maxMana = value; } }
        public int MaxManaMod { get; set; }

        public float MagicDef { get { return 1.0f / (float)(1.0f + ((float)GetStat((int)StatName.Magic_Defence).AdjustBaseValue / 100.0f)); } }
        public float MeleeDef { get { return 1.0f / (float)(1.0f + ((float)GetStat((int)StatName.Melee_Defence).AdjustBaseValue / 100.0f)); } }


        public bool Dead { get; private set; }

        public Entity SelfEntityRef { get;  set; }

        public void Init()
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
    


        //TODO Delete 
        //public virtual void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        //{
        //    SelfEntityRef = entity;
        //    dstManager.AddComponent<Unity.Transforms.CopyTransformFromGameObject>(entity);
        //    StatusBuffers = dstManager.AddBuffer<EffectStatusBuffer>(entity);
        //}

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

            SetupVitalBase();
            SetupVitalModifiers();
        }
        private void SetupAbilities()
        {
            for (int cnt = 0; cnt < _ability.Length; cnt++)
                _ability[cnt] = new Abilities();
            SetupAbilitesBase();
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
            SetupStatsBase();
            SetupStatsModifiers();
        }

        public Stat GetStat(int index)
        {
            return _stats[index];
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
        }
 
        public List<Attributes> GetAttributes()
        {
            List<Attributes> temp = new List<Attributes>();
            foreach (Attributes att in _primaryAttribute)
            {
                temp.Add(att);
            }
            return temp;
        }
        public List<Vital> GetVitals()
        {
            List<Vital> temp = new List<Vital>();
            foreach (Vital att in _vital)
            {
                temp.Add(att);
            }
            return temp;
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

        //public void AddStatus(StatusEffect StatusToAdd) { }
        //public void RemoveStatus(StatusEffect StatusToAdd) { }
        //public void RemoveStatus(EffectStatus StatusName) { }

        public async void SetAttributes(int[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                _primaryAttribute[i].BaseValue = data[i];
            }
            await Task.Delay(TimeSpan.FromSeconds(1));
            StatUpdate();
        }
        public bool SetAlteredStatus(AlteredStatus statusToChangeTo) { return false; } //Todo make a bool 


        public abstract void TakeDamage(int Amount, TypeOfDamage typeOf, Element element);
        public abstract void ReactToHit(float impact, Vector3 Test, Vector3 Forward, TypeOfDamage typeOf = TypeOfDamage.Melee, Element element = Element.None);
    }


}
