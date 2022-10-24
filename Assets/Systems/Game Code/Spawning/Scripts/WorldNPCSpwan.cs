using BestiaryLibrary;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IAUS.NPCScriptableObj {

    [CreateAssetMenu(fileName = "City NPC Data",menuName ="ScriptableObject/City Data" , order = 1)]
    public  class WorldNPCSpwan : ScriptableObject
    {
        public List<CitizenNPC> Citizens { get { return citizens; } }
        [SerializeField] List<CitizenNPC> citizens;

        public async void SpawnWorld(Vector3 pos) {
            foreach (CitizenNPC citizen in Citizens)
                for (int i = 0; i < citizen.Count; i++)
                {
                    BestiaryDB.SpawnNPCGOandCreateDataEntity(pos, citizen.GetPhysicsInfo, citizen.Self, citizen.GetPerceptibility,citizen.GetInfluence , citizen.AIStatesAvailable);
                    await Task.Delay(25);
                }
            await Task.Delay(25);
        }
    }
}