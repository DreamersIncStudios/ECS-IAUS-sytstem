using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AISenses;

namespace IAUS.NPCSO.editor
{
    public sealed partial class NPCEditor : EditorWindow
    {
        Vision GetVision;
        bool ShowVision;
        Vision SetupVision() {
            ShowVision = EditorGUILayout.BeginFoldoutHeaderGroup(ShowVision, "Vision Data");
            if (ShowVision)
            {
               GetVision.HeadPositionOffset = EditorGUILayout.Vector3Field("Head Position Offset", GetVision.HeadPositionOffset);
                GetVision.viewRadius = EditorGUILayout.Slider("View Radius ",GetVision.viewRadius,5, 75);
            GetVision.ViewAngle = EditorGUILayout.IntSlider("View Angle", GetVision.ViewAngle, 30, 180);
                GetVision.EngageRadius = EditorGUILayout.Slider("Engage Radius", GetVision.EngageRadius, .5f, 25);
                GetVision.Scantimer = EditorGUILayout.Slider("Look Rate", GetVision.Scantimer, 1, 10);
            }


            EditorGUILayout.EndFoldoutHeaderGroup();

            return GetVision;
        }

    }
}