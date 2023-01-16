using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Core.SaveSystems;

namespace DreamersInc.ComboSystem
{
    [CreateAssetMenu(fileName = "Combo", menuName = "ComboSystem/Combo Data")]

    public class ComboSO : ScriptableObject, ICombos
    {

        [SerializeField] List<ComboSingle> _comboLists;
        [HideInInspector] public List<ComboSingle> ComboLists { get { return _comboLists; } }

        public TextAsset ComboNamesText;
        public int ComboListIndex; 
        public void UnlockCombo(ComboNames Name)
        {
            //TODO Implement Unlocking System
        }

        public bool GetAnimationTrigger(AnimatorStateInfo State, ComboInfo info, out AnimationTrigger trigger, out float endtime)
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
           
            throw new ArgumentOutOfRangeException("Animation not registered in Combo SO System");

        }
        public int GetAnimationComboIndex(string state)
        {
     
            throw new ArgumentOutOfRangeException("Animation not registered in Combo SO System");
        }
 
        #endregion

   

    public List<string> GetListOfComboNames() { 
            List<string> list = new();
            var lines = ComboNamesText.text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var parts =  lines[ComboListIndex].Split(';');
            foreach (var part in parts) { 
                list.Add(part);
            }
            return list;
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
        public ComboNames name;
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
        public string name;
        public ComboNames ComboEnumName;
        public bool Unlocked { get; set; }
        [NonReorderable] public Queue<AttackType> test;
    }
    [System.Serializable]
    public class ComboSingle {
        [SerializeField] ComboNames name;
        public ComboNames Name { get { return name; } set { name = value; } } // Change To String ???????????
        public bool Unlocked;
        

        [SerializeField] List<AnimationCombo> comboList;
        [HideInInspector] public List<AnimationCombo> ComboList { get { return comboList; } }
    }

}