using Unity.Mathematics;
using Unity.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using DreamerInc.CombatSystem;
using Random = UnityEngine.Random;
using DreamersInc.CharacterControllerSys.VFX;

namespace DreamersInc.ComboSystem
{

    [System.Serializable]
    public struct AnimationCombo

    {
        public AttackType CurrentAnim;
        [SerializeField] uint triggerAnimIndex;
        public bool Alternate;
        public uint TriggerAnimIndex { get { return triggerAnimIndex; } set { triggerAnimIndex = value; } }
        public string CurrentStateName { get { return CurrentAnim.ToString() + TriggerAnimIndex; } }
        public float2 NormalizedInputTime;
        public float AnimationEndTime;
        public bool InputAllowed(float time) => time > NormalizedInputTime.x && time < NormalizedInputTime.y;
        public float MaxProb { get; set; }
        public AnimationTrigger Trigger;
        // TODO consider adding late inputs ??????

    }

    public interface ITrigger
    {
        public ComboNames Name { get; } // Change To String ???????????
        public string TriggerString { get; }
    }
    [System.Serializable]
    public struct AnimationTrigger : ITrigger
    {
        ComboNames name;
        public ComboNames Name { get { return name; } } // TODO Remove
        public uint TriggerAnimIndex { get { return triggerAnimIndex; } set { triggerAnimIndex = value; } }
        public AttackType attackType;
        public uint triggerAnimIndex;
        public string TriggerString { get { return attackType.ToString() + TriggerAnimIndex; } }
        public float TransitionDuration;
        public float TransitionOffset;
        public float EndofCurrentAnim;
        public VFX AttackVFX;
        [Tooltip(" testing Value")]
        public float Chance;
        [Range(-1, 100)]
        [Tooltip("Value Must be between 0 and 100 \n " +
            "-1 is used for never repeat")]

        public int ChanceForNextAttack;
        public float ProbabilityTotalWeight { get; set; }

        //   float probabilityPercent => Chance / ProbabilityTotalWeight * 100;
        public float ProbabilityRangeFrom { get; set; }
        float probabilityRangeTo => ProbabilityRangeFrom + Chance;
        public void SetRangeFrom(float StartPoint)
        {
            ProbabilityRangeFrom = StartPoint;
        }
        public bool Picked(float picked)
        {
            return picked > ProbabilityRangeFrom && picked < probabilityRangeTo;
        }

        //TODO add stat modifer increase or decrese likely hood of sequenctial attack based on stats
        public bool AttackAgain(float selected)
        {
            return selected < ChanceForNextAttack && ChanceForNextAttack != -1;
        }
        public float delay;
        public bool Trigger => delay <= 0.0f;
        public void AdjustTime(float time)
        {
            delay -= time;
        }

    }
    [Serializable]
    public struct VFX
    {
        public int ID;
        public float Forward, Up;
        public Vector3 Rot;
        [Tooltip("Time in Milliseconds")]
        public float LifeTime;
        [Range(0, 100)]
        public int ChanceToPlay;
        public bool Play => ID != 0;
        public void SpawnVFX(Transform CharacterTranform)
        {
            int prob = Mathf.RoundToInt(Random.Range(0, 99));
            if (prob < ChanceToPlay)
            {
                Vector3 forwardPos = CharacterTranform.forward * Forward + CharacterTranform.up * Up;
                VFXDatabase.Instance.PlayVFX(ID, CharacterTranform.position + forwardPos, CharacterTranform.rotation.eulerAngles + Rot, 0, LifeTime);
            }
        }
    }


    [System.Serializable]
    public struct SetTrigger
    {
        [SerializeField] ComboNames name;
        [SerializeField] ComboAnimNames triggerAnimName;

        public ComboNames Name { get { return name; } } // Change To String ???????????
        public ComboAnimNames TriggeredAnimName { get { return triggerAnimName; } } // Change to String ???????????

    }
    [System.Serializable]
    public struct ComboPattern
    {
        public ComboNames name;
        public List<SetTrigger> Attacks;
    }
    public enum ComboAnimNames
    {
        None, Grounded, Targeted_Locomation, Locomation_Grounded_Weapon,
        Equip_Light, Equip_Heavy, Equip_LightCharged, Equip_HeavyCharged, Equip_Projectile,
        Light_Attack1, Light_Attack2, Light_Attack3, Light_Attack4, Light_Attack5, Light_Attack6,
        Heavy_Attack1, Heavy_Attack2, Heavy_Attack3, Heavy_Attack4, Heavy_Attack5, Heavy_Attack6
            , Ground_attack02, Light_Attack1_Alt, Projectile, ChargedProjectile
    }
    public enum AttackType
    {
        none, LightAttack, HeavyAttack, ChargedLightAttack, ChargedHeavyAttack, Projectile, ChargedProjectile, Grounded, Targeted_Locomation, Locomation_Grounded_Weapon,
        SpecialAttack, Defend, Dodge

    }
    public enum ComboNames
    {
        None, Combo_1, Combo_2, Combo_3, Combo_4, Combo_5, Combo_6, Combo_7, Combo_8, Combo_9, Combo_10,
        Projectile1, Dodge,
    }
}