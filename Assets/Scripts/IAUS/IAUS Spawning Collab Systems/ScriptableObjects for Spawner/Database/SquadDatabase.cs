using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpawnerSystem.ScriptableObjects;
namespace SpawnerSystem
{
    static public class SquadDatabase
    {
        static public List<SquadSO> Squads;
        static public bool IsLoaded { get; private set; } = false;

        static private void ValidateDatebase()
        {
            if (Squads == null) Squads = new List<SquadSO>();
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
            SquadSO[] resources = Resources.LoadAll<SquadSO>(@"Squad");
            foreach (SquadSO squad in resources)
            {
                if (!Squads.Contains(squad))
                    Squads.Add(squad);
            }
        }

        static public void ClearDatabase()
        {
            IsLoaded = false;
            Squads.Clear();
        }

        static public SquadSO GetEnemy(int SpawnID)
        {
            ValidateDatebase();
            LoadDatabase();
            foreach (SquadSO squad in Squads)
            {
                if (squad.SpawnID == SpawnID)
                    return ScriptableObject.Instantiate(squad) as SquadSO;
            }
            return null;
        }
    }
}