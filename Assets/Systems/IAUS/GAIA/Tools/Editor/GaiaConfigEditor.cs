using UnityEditor;
using UnityEngine;

namespace DreamersIncStudio.GAIACollective.EditorWindows
{
    [CustomEditor(typeof(GaiaConfiguration))]
    public class GaiaConfigEditor : Editor
    {
        private GameObject sun; // Cached reference to the "Sun" GameObject

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GaiaConfiguration config = (GaiaConfiguration)target;
            GUILayout.Space(10);
            GUILayout.Label("🔆 Set Time of Day", EditorStyles.boldLabel);

            // Find "Sun" GameObject by tag
            if (GUILayout.Button("🔍 Find Sun by Tag"))
            {
                sun = GameObject.FindWithTag("Sun");

                if (sun == null)
                {
                    Debug.LogWarning("No GameObject found with the tag 'Sun'. Ensure the GameObject is tagged before using this tool.");
                }
                else
                {
                    Debug.Log($"Found Sun GameObject: {sun.name}");
                }
            }

            if (GUILayout.Button("🌅 Set to Daybreak"))
            {
                config.SetToDaybreak(sun);
            }

            if (GUILayout.Button("☀ Set to Midday"))
            {
                config.SetToMidday(sun);
            }

            if (GUILayout.Button("🌇 Set to Sunset"))
            {
                config.SetToSunset(sun);
            }

            if (GUILayout.Button("🌙 Set to Night"))
            {
                config.SetToNight(sun);
            }
        }
    }
}