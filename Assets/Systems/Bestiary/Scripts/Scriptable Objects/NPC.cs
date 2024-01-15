using System.Collections;
using System.Collections.Generic;
using Stats;
using UnityEngine;
using IAUS;
using MotionSystem.Components;
using Sirenix.OdinInspector;

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

        private bool patrolStateInclude => AIStatesToAdd.Contains(AIStates.Patrol);
        [ShowIf("patrolStateInclude")] 
        [SerializeField] private bool selectPatrolPoints;

        [ShowIf("selectPatrolPoints")] [SerializeField] private List<Vector3> waypoints;
#if UNITY_EDITOR

        public void setItemID(uint ID)
        {

            this.creatureID = ID;
        }
#endif
    }
    
    
    
}