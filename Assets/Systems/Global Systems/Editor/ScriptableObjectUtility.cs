using UnityEngine;
using UnityEditor;
using System.IO;


namespace Dreamers.Global
{
    public static class ScriptableObjectUtility
    {
        public static void CreateAsset<T>(string folderPath, string pathExt, out T test) where T : ScriptableObject {
            // Ensure the folder path exists. Unity does this automatically.
            if (!System.IO.Directory.Exists(folderPath))
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }
            T asset = ScriptableObject.CreateInstance<T>();
          
            string assetPath = $"{folderPath}/{pathExt}.asset";

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            test = asset;
        }

    }
}