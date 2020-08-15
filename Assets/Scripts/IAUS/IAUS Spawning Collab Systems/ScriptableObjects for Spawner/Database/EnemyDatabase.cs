using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpawnerSystem.ScriptableObjects;
namespace SpawnerSystem
{
    
 static public class EnemyDatabase
    {
        static public List<Enemy> _Enemies;
        static public bool IsLoaded{get; private set;} = false;

        static private void ValidateDatebase() {
            if (_Enemies == null) _Enemies = new List<Enemy>();
        }

        static public void LoadDatabase() {
            if (IsLoaded)
                return;
            LoadDatabaseForce();
        }
        static public void LoadDatabaseForce() {
            ValidateDatebase();
            IsLoaded = true;
            Enemy[] resources = Resources.LoadAll<Enemy>(@"Enemy");
            foreach (Enemy enemy in resources)
            {
                if (!_Enemies.Contains(enemy))
                    _Enemies.Add(enemy);
            }
        }

        static public void ClearDatabase() {
            IsLoaded = false;
            _Enemies.Clear();
        }

        static public Enemy GetEnemy(int SpawnID) {
            ValidateDatebase();
            LoadDatabase();
            foreach (Enemy enemy in _Enemies)
            {
                if (enemy.SpawnID == SpawnID)
                    return ScriptableObject.Instantiate(enemy) as Enemy;    
            }
            return null;
        }
    }
    static public class ItemDatabase
    {
        static public List<Droppable> droppables;
        static public bool isLoaded { get; private set; } = false;

        static private void ValidateDatebase()
        {
            if (droppables== null) 
                droppables= new List<Droppable>();
        }

        static public void LoadDatabase() {
            if (isLoaded)
                return;
            LoadDatabaseForce();
        }
        static public void LoadDatabaseForce()
        {
            ValidateDatebase();
            isLoaded = true;
            RecoveryItemSO[] resources = Resources.LoadAll<RecoveryItemSO>(@"Item");
            foreach (RecoveryItemSO item  in resources)
            {
                if (!droppables.Contains((Droppable)item))
                    droppables.Add(item);
            }
        }

        static public void ClearDatabase()
        {
            isLoaded = false;
            droppables.Clear();
        }

        static public Droppable GetItem(int SpawnID)
        {
            ValidateDatebase();
            LoadDatabase();
            foreach (Droppable drop in droppables)
            {
                if (drop.SpawnID == SpawnID)
                    return ScriptableObject.Instantiate(drop) as Droppable;
            }
            return null;
        }

    }

}