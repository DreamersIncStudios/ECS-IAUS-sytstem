using UnityEngine;
using UnityEditor;
using System.IO;
#if UNITY_EDITOR

namespace DreamersInc.FactionSystem.Wrappers
{
    public static class FactionCoreMenuItems
    {
        [MenuItem("Assets/Create/Dreamer's Inc/Factions/Faction Database", false, 0)]
        public static void CreateFactionDatabase() {
            var asset = ScriptableObject.CreateInstance<FactionDatabase>() as FactionDatabase;
            asset.Initialize();
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                path = "Assets";
            }
            else if (!string.IsNullOrEmpty(Path.GetExtension(path))) 
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New Faction Database.asset");
                       AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}
#endif