using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace IAUS.Editors
{
    public class UTEditorWindow : EditorWindow
    {
        [MenuItem("IAUS/Utility Window")]
        public static void ShowWindow() {
            EditorWindow.GetWindow(typeof(UTEditorWindow));
        }

        Vector2 scrollPosition;

        private void OnGUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (GameObject GO in Selection.gameObjects)
            {
                UtilitySystem US = GO.GetComponent<UtilitySystem>();
                if (US != null)
                {

                 
                    GUILayout.BeginVertical("box");

                    GUILayout.Label(GO.name);

                    foreach (ActionBase Action in US.Actions)
                    {
                        GUILayout.BeginVertical("box");
                        GUILayout.Label(Action.NameId + ": " + Action.TotalScore);
                        foreach (ConsiderationBase consideration in Action.Considerations)
                        {
                            GUILayout.Label("\t" + consideration.NameId + ": " + consideration.Score);
                        }
                        GUILayout.EndVertical();

                    }

                    GUILayout.EndVertical();
                }
            }
                GUILayout.EndScrollView();
            
        }

    }
}