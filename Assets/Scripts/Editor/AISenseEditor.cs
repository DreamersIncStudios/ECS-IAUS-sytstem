using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using AISenses;
using DreamersInc.InflunceMapSystem;

namespace IAUS.NPCSO.editor
{
    public sealed partial class NPCEditor : EditorWindow
    {
        Vision GetVision;
        bool ShowInfluence;
        public InfluenceComponent GetInfluence;

        InfluenceComponent SetupInfluence() {
            ShowInfluence = EditorGUILayout.BeginFoldoutHeaderGroup(ShowInfluence, "Influence Data");
            if (ShowInfluence) 
            {
                GetInfluence.Threat = EditorGUILayout.IntField("Threat", GetInfluence.Threat);
                GetInfluence.Protection = EditorGUILayout.IntField("Protection", GetInfluence.Protection);
                GetInfluence.factionID = EditorGUILayout.IntField("Faction Group Member", GetInfluence.factionID);// TODO Change to a custom dropdown based off Database later
            }

            return GetInfluence;
        }


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