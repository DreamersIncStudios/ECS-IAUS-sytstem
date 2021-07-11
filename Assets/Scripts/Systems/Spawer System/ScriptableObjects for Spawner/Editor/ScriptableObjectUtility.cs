﻿
using UnityEngine;
using UnityEditor;
using System.IO;
#if UNITY_EDITOR

namespace SpawnerSystem.Editors
{
    static public class ScriptableObjectUtility
    {
        static public void CreateAsset<T>(string pathExt, out T test) where T : ScriptableObject {
            T asset = ScriptableObject.CreateInstance<T>();
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets/" + pathExt;
            }
            else if (Path.GetExtension(path) != "") {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/New" + ".asset");
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            test = asset;
        }


        static public void CreateAsset<T>(string pathExt, string NameOfFile, out T test) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets/" + pathExt;
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + NameOfFile+ ".asset");
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            test = asset;
        }
    }
}
#endif