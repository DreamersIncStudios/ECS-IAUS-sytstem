using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

/*Generic Player Character Class
 * Will be using BurgZergArcade Derived Player Character System  that is already in main project file
 */
namespace Stats
{

    //Should this be Buffer? Adding new ComponentData Overwrites existing Data
    public struct ChangeVitalBuffer : IBufferElementData 
    {
        public VitalChange recover;

        public static implicit operator VitalChange(ChangeVitalBuffer e) { return e.recover; }
        public static implicit operator ChangeVitalBuffer(VitalChange e) { return new ChangeVitalBuffer { recover = e }; }

    }



    public enum VitalType {
        Health, Mana, Both
    }
    public struct VitalChange 
    {
        public VitalType type;
        public bool Increase;
        public int value;
        public uint Iterations;
        public float Frequency;
        public float Timer;
    }
 

    // add safe checks
    public struct PlayerStatComponent: IComponentData {
        [Range(0, 999)]
        [SerializeField] int _curHealth;
        public int CurHealth { get { return _curHealth; } 
            set {

                if (value <= 0)
                    _curHealth = 0;
                else if (value > MaxHealth)
                    _curHealth = MaxHealth;
                else
                    _curHealth = value;
            } }
        [Range(0, 999)]
        [SerializeField] int _curMana;

        public int CurMana
        {
            get { return _curMana; }
            set
            {

                if (value <= 0)
                    _curMana= 0;
                else if (value > MaxMana)
                    _curMana = MaxMana;
                else
                    _curMana = value;
            }
        }
        [Range(0, 999)]
        public int MaxHealth;
        [Range(0, 999)]
        public int MaxMana;

        public float MagicDef;
        public float MeleeAttack;
        public float MeleeDef;
    }

}