using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SpawnerSystem.Editors;

namespace ProjectRebirth.Bestiary
{
    static public class NPCDatabase
    {
        static public List<NPCBase> _NPCs;
        static public bool IsLoaded { get; private set; } = false;

        static private void ValidateDatebase()
        {
            if (_NPCs == null) _NPCs = new List<NPCBase>();
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
            NPCBase[] resources = Resources.LoadAll<NPCBase>(@"NPC");
            foreach (NPCBase enemy in resources)
            {
                if (!_NPCs.Contains(enemy))
                    _NPCs.Add(enemy);
            }
        }


        static public void ClearDatabase()
        {
            IsLoaded = false;
            _NPCs.Clear();
        }

        static public NPCBase GetEnemy(int SpawnID)
        {
            ValidateDatebase();
            LoadDatabase();
            foreach (NPCBase enemy in _NPCs)
            {
                if (enemy.SpawnID == SpawnID)
                    return ScriptableObject.Instantiate(enemy) as NPCBase;
            }
            return null;
        }

        [MenuItem("Assets/Create/Game/NPC")]
        static public void CreateEnemyNPC()
        {
            EnemyNPC enemyNPC;
            ScriptableObjectUtility.CreateAsset<EnemyNPC>("Resources / NPC", out enemyNPC);
            NPCDatabase.LoadDatabaseForce();
            if (NPCDatabase._NPCs.Count == 0)
                enemyNPC.SpawnID = 0;
            else
                enemyNPC.SpawnID = NPCDatabase._NPCs[NPCDatabase._NPCs.Count-1].SpawnID;
        }

    }


}