using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using IAUS;

namespace IAUS.Editors
{
    [CustomEditor(typeof(CharacterContext))]
    [CanEditMultipleObjects]
    public class EnemyDetectionEditor : Editor
    {

        private void OnSceneGUI()
        {
            CharacterContext FOW = (CharacterContext)target;
            Handles.color = Color.white;
            Handles.color = Color.white;
            Handles.DrawWireArc(FOW.transform.position, Vector3.up, Vector3.forward, 360, FOW.viewRadius);
            Vector3 ViewAngleA = FOW.DirFromAngle(-FOW.viewAngle / 2, false);
            Vector3 ViewAngleB = FOW.DirFromAngle(FOW.viewAngle / 2, false);
            Handles.DrawLine(FOW.transform.position, FOW.transform.position + ViewAngleA * FOW.viewRadius);
            Handles.DrawLine(FOW.transform.position, FOW.transform.position + ViewAngleB * FOW.viewRadius);

            Handles.color = Color.blue;
            Handles.DrawWireArc(FOW.transform.position, Vector3.up, Vector3.forward, 360, FOW.EngageRadius);
            Vector3 ViewAngleC = FOW.DirFromAngle(-FOW.EngageViewAngle / 2, false);
            Vector3 ViewAngleD = FOW.DirFromAngle(FOW.EngageViewAngle / 2, false);

            Handles.DrawLine(FOW.transform.position, FOW.transform.position + ViewAngleC * FOW.EngageRadius);
            Handles.DrawLine(FOW.transform.position, FOW.transform.position + ViewAngleD * FOW.EngageRadius);

            Handles.color = Color.red;
            foreach (Transform visibleTarget in FOW.VisibleTargets)
            {
                Handles.DrawLine(FOW.transform.position, visibleTarget.position);

            }



        }
    }
}