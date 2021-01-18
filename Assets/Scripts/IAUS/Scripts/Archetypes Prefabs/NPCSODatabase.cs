using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAUS.SO
{
    public static  class NPCSODatabase 
    {
        static public List<NPCSO> NPCs;
        static public bool IsLoaded { get; private set; } = false;

        static private void ValidateDatebase()
        {
            if (NPCs == null) NPCs = new List<NPCSO>();
        }

        static public void LoadDatabase()
        {
            if (IsLoaded)
                return;
            LoadDatabaseForce();
        }
        static public void LoadDatabaseForce()
        {
            ValidateDatebase();
            IsLoaded = true;
            NPCSO[] resources = Resources.LoadAll<NPCSO>(@"NPC SO AI");
            foreach (NPCSO squad in resources)
            {
                if (!NPCs.Contains(squad))
                    NPCs.Add(squad);
            }
        }

        static public void ClearDatabase()
        {
            IsLoaded = false;
            NPCs.Clear();
        }

        static public NPCSO GetEnemy(int SpawnID)
        {
            ValidateDatebase();
            LoadDatabase();
            foreach (NPCSO squad in NPCs)
            {
                if (squad.SpawnID == SpawnID)
                    return ScriptableObject.Instantiate(squad) as NPCSO;
            }
            return null;
        }
    }
}