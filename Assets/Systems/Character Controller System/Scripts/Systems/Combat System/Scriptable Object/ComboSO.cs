using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

//using Core.SaveSystems;

namespace DreamersInc.ComboSystem
{
    [CreateAssetMenu(fileName = "Combo", menuName = "ComboSystem/Combo Data")]

    // ReSharper disable once InconsistentNaming
    public class ComboSO : ScriptableObject, ICombos
    {

        [FormerlySerializedAs("_comboLists")] [SerializeField] List<ComboSingle> comboLists;
        [HideInInspector] public List<ComboSingle> ComboLists => comboLists;

        public TextAsset ComboNamesText;
        public int ComboListIndex; 
        public void UnlockCombo(ComboNames name)
        {
            //TODO Implement Unlocking System
        }

        public bool GetAnimationTrigger(AnimatorStateInfo state, ComboInfo info, out AnimationTrigger trigger, out float endtime)
        {
            endtime = 0.0f;
            trigger = new AnimationTrigger();
            // TODO Implement
            return false;

        }

        public bool ShowMovesPanel = false;

        //ComboSaveData ComboSave = new ComboSaveData();
        //public SaveData GetSaveData()
        //{

        //    throw new System.NotImplementedException();
        //}

        public void Load(string json)
        {
            throw new System.NotImplementedException();
        }

        #region NPC Attack system
        public void OnValidate()
        {
            UpdateTotalProbability();

        }
        public void UpdateTotalProbability()
        {
           
        }
        public AnimationTrigger GetTrigger(AnimatorStateInfo state) {
            foreach (ComboSingle combo in ComboLists) {
                foreach (AnimationCombo test in combo.ComboList) {
                    if(state.IsName(test.Trigger.TriggerString))
                        return test.Trigger;
                }
            }

            return new AnimationTrigger();
        }
        public VFX GetVFX(AnimatorStateInfo state)
        {
            return GetTrigger(state).AttackVFX;
        }
        public int GetAnimationComboIndex(AnimatorStateInfo state) {
           
            throw new ArgumentOutOfRangeException(nameof(state));

        }
        public int GetAnimationComboIndex(string state)
        {
     
            throw new ArgumentOutOfRangeException(nameof(state));
        }
 
        #endregion

   

    public List<string> GetListOfComboNames() {
        var lines = ComboNamesText.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var parts =  lines[ComboListIndex].Split(';');
            return parts.ToList();
        }
        public List<ComboDefinition> GetComboDefinitions() {
            List<ComboDefinition> temp = new ();
           
            
            return temp;
        }
        public void DisplayCombo()
        {
           List<ComboDefinition> comboDefinitions = GetComboDefinitions();
            // Launch Modal Window 
        }
    }
   
    
    
    
    
    [System.Serializable]
    public struct ComboInfo
    {
        [FormerlySerializedAs("name")] public ComboNames Name;
        public bool Unlocked;
    }
    //[System.Serializable]
    //public class ComboSaveData : SaveData
    //{
    //    [NonReorderable] public List<ComboInfo> SaveData;
    //}

    [System.Serializable]
    public class ComboDefinition
    {
        [FormerlySerializedAs("name")] public string Name;
        public ComboNames ComboEnumName;
        public bool Unlocked { get; set; }
        [NonReorderable] public Queue<AttackType> Test;
    }
    [System.Serializable]
    public class ComboSingle {
        [SerializeField] ComboNames name;
        public ComboNames Name { get { return name; } set { name = value; } } // Change To String ???????????
        public bool Unlocked;
        

        [SerializeField] List<AnimationCombo> comboList;
        [HideInInspector] public List<AnimationCombo> ComboList { get { return comboList; } }
    }
    
    public struct AIComboInfo
    {
        public int Chance;
        public ComboNames AttackName;
        public AnimationTrigger Trigger;
    }
}