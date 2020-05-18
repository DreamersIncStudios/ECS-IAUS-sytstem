using UnityEditor;
using SpawnerSystem.ScriptableObjects;
namespace SpawnerSystem.Editors
{

    static public class SOEditor
    {
        [MenuItem("Assets/Create/RPG/Enemy")]

        static public void CreateEnemy() {
            Enemy enemy;
            ScriptableObjectUtility.CreateAsset<Enemy>("Enemy" ,  out enemy);
            EnemyDatabase.LoadDatabaseForce();
            enemy.SpawnID = EnemyDatabase._Enemies.Count + 1;
        }

        [MenuItem("Assets/Create/RPG/Recovery Item")]

        static public void CreateRecoveryItem()
        {
            RecoveryItemSO Item;
            ScriptableObjectUtility.CreateAsset<RecoveryItemSO>("Item", out Item);
           ItemDatabase.LoadDatabaseForce();
            Item.SpawnID = ItemDatabase.droppables.Count + 1;
        }
    }
}