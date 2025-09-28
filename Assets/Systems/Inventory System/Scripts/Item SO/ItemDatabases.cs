using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Dreamers.InventorySystem.Interfaces;

#if UNITY_EDITOR
using Dreamers.Global;
#endif

namespace Dreamers.InventorySystem
{
    public static class ItemDatabase 
    {
        static public List<ItemBaseSO> Items;
        static public bool isLoaded { get; private set; }

        private static void ValidateDatabase() {
            if (Items == null||!isLoaded )
            {
                Items = new List<ItemBaseSO>();
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
            Items = new List<ItemBaseSO>();
            isLoaded = true;
            ItemBaseSO[] itemsToLoad = Resources.LoadAll<ItemBaseSO>(@"Items");
            foreach (var item in itemsToLoad)
            {
                if (!Items.Contains(item))
                    Items.Add(item);
            }
        }
        public static void ClearDatabase() {
            isLoaded = false;
            Items.Clear();

        }
        public static ItemBaseSO GetItem(int SpawnID) {
            ValidateDatabase();
            LoadDatabase();
            foreach (ItemBaseSO item in Items)
            {
                if (item.ItemID == SpawnID)
                    return ScriptableObject.Instantiate(item) as ItemBaseSO;
                // Consider add switch to return Item as it proper type ?????

            }
            return null;
        }

#if UNITY_EDITOR
        public static class Creator {

            //[MenuItem("Assets/Create/RPG/Recovery Item")]
            //static public void CreateRecoveryItem()
            //{
            //    ScriptableObjectUtility.CreateAsset<RecoveryItemSO>("Item", out RecoveryItemSO Item);
            //    ItemDatabase.LoadDatabaseForce();
            //    Item.setItemID((uint)ItemDatabase.Items.Count + 1);
            //    Debug.Log( Item.ItemID );
            //    // need to deal with duplicate itemID numbers 

            //}
            private const string ArmorFolderPath = "Assets/Prefab Library/Resources/Item Database/Weapons";

            [MenuItem("Assets/Create/RPG/Armor Item")]
            public static void CreateArmorItem()
            {
                ScriptableObjectUtility.CreateAsset<ArmorSO>(ArmorFolderPath,"Armor", out ArmorSO Item);
                ItemDatabase.LoadDatabaseForce();
                Item.setItemID();
                Debug.Log(Item.ItemID);
                AssetDatabase.SetLabels(Item, new [] {"Item","Equippable","Armor"});
                // need to deal with duplicate itemID numbers 

            }
            private const string WeaponFolderPath = "Assets/Prefab Library/Resources/Item Database/Weapons";

            [MenuItem("Assets/Create/RPG/Weapon/Melee Item")]
            public static void CreateWeaponItem()
            {
                ScriptableObjectUtility.CreateAsset<WeaponSO>(WeaponFolderPath,"Weapon", out WeaponSO Item);
                ItemDatabase.LoadDatabaseForce();
                Item.setItemID();
                Debug.Log(Item.ItemID);
                AssetDatabase.SetLabels(Item, new [] {"Item","Equippable","Weapon"});
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