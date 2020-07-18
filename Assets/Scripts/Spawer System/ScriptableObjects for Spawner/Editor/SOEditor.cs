using UnityEditor;
using UnityEngine;
using SpawnerSystem.ScriptableObjects;
using IAUS.SpawnerSystem;
using IAUS.SpawnerSystem.Database;


namespace SpawnerSystem.Editors
{

    static public class SOEditor
    {
        [MenuItem("Assets/Create/RPG/Enemy")]

        static public void CreateEnemy()
        {
            Enemy enemy;
            ScriptableObjectUtility.CreateAsset<Enemy>("Enemy", out enemy);
            EnemyDatabase.LoadDatabaseForce();
            enemy.SpawnID = EnemyDatabase._Enemies.Count + 1;
            enemy.Scale = Vector3.one;
        }

        [MenuItem("Assets/Create/RPG/Recovery Item")]

        static public void CreateRecoveryItem()
        {
            RecoveryItemSO Item;
            ScriptableObjectUtility.CreateAsset<RecoveryItemSO>("Item", out Item);
           ItemDatabase.LoadDatabaseForce();
            Item.SpawnID = ItemDatabase.droppables.Count + 1;
        }

        [MenuItem("Assets/Create/RPG/Squad")]
        static public void CreateSquadSO()
        {
            SquadSO squad;
            ScriptableObjectUtility.CreateAsset<SquadSO>("Squad", out squad);
            SquadDatabase.LoadDatabaseForce();
            squad.SpawnID = SquadDatabase.Squads.Count + 1;
        }

        [MenuItem("Assets/Create/RPG/Squad-IAUS")]
        static public void CreateIAUSSquadSO()
        {
            IAUSSquadSO squad;
            ScriptableObjectUtility.CreateAsset<IAUSSquadSO>("Squad", out squad);
            IAUSSquadDatabase.LoadDatabaseForce();
            squad.SpawnID = IAUSSquadDatabase.Squads.Count + 1;
        }
        [MenuItem("Assets/Create/RPG/Enemy-IAUS")]
        static public void CreateIAUSEnemySO()
        {
            IAUSEnemySO enemy;
            ScriptableObjectUtility.CreateAsset<IAUSEnemySO>("Enemy", out enemy);
            IAUSEnemyDatabase.LoadDatabaseForce();
            enemy.SpawnID = IAUSEnemyDatabase.Enemy.Count + 1;
        }
    }
   
}