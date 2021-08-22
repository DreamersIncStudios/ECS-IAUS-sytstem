using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAUS.NPCSO
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
            NPCSO[] resources = Resources.LoadAll<NPCSO>(@"NPC");
            EnemyNPCSO[] resources1 = Resources.LoadAll<EnemyNPCSO>(@"NPC");
            foreach (NPCSO npc in resources)
            {
                if (!NPCs.Contains(npc))
                    NPCs.Add(npc);
            }
            foreach (EnemyNPCSO enemynpc in resources1)
            {
                if (!NPCs.Contains(enemynpc))
                    NPCs.Add(enemynpc);
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