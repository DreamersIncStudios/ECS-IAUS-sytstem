using System.Collections;
using System.Collections.Generic;
using Stats;
using UnityEngine;
using IAUS;
using MotionSystem.Components;

namespace DreamersInc.Bestiary
{
    [CreateAssetMenu(fileName = "NPC Data", menuName = "Bestiary/NPC Data", order = 1)]
    public class NPC : ScriptableObject
    {
        [SerializeField] private uint creatureID;
        public uint ID { get { return creatureID; } }
        public string Name;
        public CharacterClass stats;
        public GameObject Prefab;
        public List<AIStates> AIStatesToAdd;
        public MovementData Move;
        
#if UNITY_EDITOR

        public void setItemID(uint ID)
        {

            this.creatureID = ID;
        }
#endif
    }
    
    
    
}