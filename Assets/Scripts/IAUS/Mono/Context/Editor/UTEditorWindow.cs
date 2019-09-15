//Copyright 2019 <Dreamers Inc Studios>
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


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