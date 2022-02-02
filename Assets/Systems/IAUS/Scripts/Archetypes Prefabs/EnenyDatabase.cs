using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAUS.NPCSO
{
    public static  class EnenyDatabase 
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
            EnemyNPCSO[] resources = Resources.LoadAll<EnemyNPCSO>(@"NPC");
  
            foreach (EnemyNPCSO enemynpc in resources)
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