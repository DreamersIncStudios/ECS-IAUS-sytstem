using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace AISenses.Authoring.Editors
{
    [CustomEditor(typeof(AISensesAuthoring))]
    [CanEditMultipleObjects]
    public class AISensesAuthoringEditor : Editor
    {
        private void OnSceneGUI()
        {
            AISensesAuthoring aiSenses = (AISensesAuthoring)target;
            switch (aiSenses.Editing) {
                case AISensesAuthoring.SenseToEdit.vison:
                    Handles.color = Color.white;
                    Handles.DrawWireArc(aiSenses.transform.position, Vector3.up, Vector3.forward, 360, aiSenses.VisionData.viewRadius);

                    Vector3 viewAngleA = aiSenses.DirFromAngle(-aiSenses.VisionData.ViewAngle / 2, false);
                    Vector3 viewAngleB = aiSenses.DirFromAngle(aiSenses.VisionData.ViewAngle / 2, false);

                    Handles.DrawLine(aiSenses.transform.position, aiSenses.transform.position + viewAngleA * aiSenses.VisionData.viewRadius);
                    Handles.DrawLine(aiSenses.transform.position, aiSenses.transform.position + viewAngleB * aiSenses.VisionData.viewRadius);

                    break;
                case AISensesAuthoring.SenseToEdit.hear:
                    break;
                case AISensesAuthoring.SenseToEdit.Touch:
                    break;
            }
        }
    }
}
#endif