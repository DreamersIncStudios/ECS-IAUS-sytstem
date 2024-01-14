using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Dreamers.InventorySystem.Interfaces;
using Unity.Mathematics;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using Dreamers.Global;
#endif

namespace Dreamers.InventorySystem
{
    public static class ItemDatabase 
    {
        private static List<ItemBaseSO> items;
        private static bool isLoaded { get; set; }

        private static void ValidateDatabase() {
            if (items == null||!isLoaded )
            {
                items = new List<ItemBaseSO>();
                isLoaded = false;
            }
            else { isLoaded = true; }
        }

        public static void LoadDatabase()
        {
            if (isLoaded)
                return;
            LoadDatabaseForce();
        }

        public static void LoadDatabaseForce()
        {
            items = new List<ItemBaseSO>();
            isLoaded = true;
            ItemBaseSO[] itemsToLoad = Resources.LoadAll<ItemBaseSO>(@"Items");
            foreach (var item in itemsToLoad)
            {
                if (!items.Contains(item))
                    items.Add(item);
            }
        }
        public static void ClearDatabase() {
            isLoaded = false;
            items.Clear();

        }
        public static ItemBaseSO GetItem(int SpawnID) {
            ValidateDatabase();
            LoadDatabase();
            foreach (ItemBaseSO item in items)
            {
                if (item.ItemID == SpawnID)
                    return ScriptableObject.Instantiate(item) as ItemBaseSO;
                // Consider add switch to return Item as it proper type ?????

            }
            return null;
        }

        public static void SpawnQuestItem(int ID, Vector3 positon, bool Collectable = false)
        {
            var item = (QuestSO)GetItem(ID);
            GameObject spawned = Object.Instantiate(item.Model, positon, quaternion.identity);
         
        }
#if UNITY_EDITOR
        public static class Creator {

            [MenuItem("Assets/Create/RPG/Recovery Item")]
            public static void CreateRecoveryItem()
            {
                ScriptableObjectUtility.CreateAsset<RecoveryItem>("Item", out RecoveryItem Item);
                ItemDatabase.LoadDatabaseForce();
                Item.setItemID((uint)ItemDatabase.items.Count + 1);
                Debug.Log( Item.ItemID );
                // need to deal with duplicate itemID numbers 

            }
            [MenuItem("Assets/Create/RPG/Armor Item")]
            public static void CreateArmorItem()
            {
                ScriptableObjectUtility.CreateAsset<ArmorSO>("Item", out ArmorSO Item);
                ItemDatabase.LoadDatabaseForce();
                Item.setItemID((uint)ItemDatabase.items.Count + 1);
                Debug.Log(Item.ItemID);
                // need to deal with duplicate itemID numbers 

            }
            [MenuItem("Assets/Create/RPG/Weapon Item")]
            public static void CreateWeaponItem()
            {
                ScriptableObjectUtility.CreateAsset<WeaponSO>("Item", out WeaponSO Item);
                ItemDatabase.LoadDatabaseForce();
                Item.setItemID((uint)ItemDatabase.items.Count + 1);
                Debug.Log(Item.ItemID);
                // need to deal with duplicate itemID numbers 

            }
            [MenuItem("Assets/Create/RPG/Quest Item")]
            public static void CreateQuestItem()
            {
                ScriptableObjectUtility.CreateAsset<QuestSO>("Item", out var item);
                ItemDatabase.LoadDatabaseForce();
                item.setItemID((uint)ItemDatabase.items.Count + 1);
                Debug.Log(item.ItemID);
                // need to deal with duplicate itemID numbers 

            }
            //[MenuItem("Assets/Create/RPG/Projectile Item")]
            //     static public void CreateBlasterItem()
            //{
            //    ScriptableObjectUtility.CreateAsset<BlasterSO>("Item", out BlasterSO Item);
            //    ItemDatabase.LoadDatabaseForce();
            //    Item.setItemID((uint)ItemDatabase.Items.Count + 1);
            //    Debug.Log(Item.ItemID);
            //    // need to deal with duplicate itemID numbers 

            //}
        }
#endif
    }
}