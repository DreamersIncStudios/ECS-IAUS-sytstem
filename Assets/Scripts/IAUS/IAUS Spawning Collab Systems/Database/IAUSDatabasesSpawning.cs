using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAUS.SpawnerSystem.Database {
    static public class IAUSSquadDatabase
    {
        static public List<IAUSSquadSO> Squads;
        static public bool IsLoaded { get; private set; } = false;

        static private void ValidateDatebase()
        {
            if (Squads == null) Squads = new List<IAUSSquadSO>();
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
            IAUSSquadSO[] resources = Resources.LoadAll<IAUSSquadSO>(@"Enemy-IAUS");
            foreach (IAUSSquadSO squad in resources)
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

        static public IAUSSquadSO GetSquad(int SpawnID)
        {
            ValidateDatebase();
            LoadDatabase();
            foreach (IAUSSquadSO squad in Squads)
            {
                if (squad.SpawnID == SpawnID)
                    return ScriptableObject.Instantiate(squad) as IAUSSquadSO;
            }
            return null;
        }
    }

    static public class IAUSEnemyDatabase // rename
    {
        static public List<IAUSEnemySO> Enemy;
        static public bool IsLoaded { get; private set; } = false;

        static private void ValidateDatebase()
        {
            if (Enemy == null) Enemy = new List<IAUSEnemySO>();
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
            IAUSEnemySO[] resources = Resources.LoadAll<IAUSEnemySO>(@"Enemy-IAUS");
            foreach (IAUSEnemySO enemy in resources)
            {
                if (!Enemy.Contains(enemy))
                    Enemy.Add(enemy);
            }
        }

        static public void ClearDatabase()
        {
            IsLoaded = false;
            Enemy.Clear();
        }

        static public IAUSEnemySO GetEnemy(int SpawnID)
        {
            ValidateDatebase();
            LoadDatabase();
            foreach (IAUSEnemySO enemy in Enemy)
            {
                if (enemy.SpawnID == SpawnID)
                    return ScriptableObject.Instantiate(enemy) as IAUSEnemySO;
            }
            return null;
        }
    }

}