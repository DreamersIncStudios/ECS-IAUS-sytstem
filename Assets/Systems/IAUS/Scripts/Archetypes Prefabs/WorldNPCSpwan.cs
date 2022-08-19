using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAUS.NPCScriptableObj {

    [CreateAssetMenu(fileName = "City NPC Data",menuName ="ScriptableObject/City Data" , order = 1)]
    public  class WorldNPCSpwan : ScriptableObject
    {
        public List<CitizenNPC> Citizens { get { return citizens; } }
        [SerializeField] List<CitizenNPC> citizens;

        public void SpawnWorld(Vector3 pos) {
            foreach (CitizenNPC citizen in Citizens)
                for (int i = 0; i < citizen.Count; i++)
                {
                    citizen.SpawnDataEntity(pos);
                }
        }
    }
}