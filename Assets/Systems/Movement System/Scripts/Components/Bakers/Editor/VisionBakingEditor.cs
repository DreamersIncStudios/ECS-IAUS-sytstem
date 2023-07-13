using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


#if UNITY_EDITOR

namespace DreamersInc.MovementSys.Editors
{

    [CustomEditor(typeof(VisionBaking))]
    [CanEditMultipleObjects]
    public class VisionBakingEditor : Editor
    {
        private void OnSceneGUI()
        {
            VisionBaking aiSenses = (VisionBaking)target;

            Handles.color = Color.white;
            Handles.DrawWireArc(aiSenses.transform.position, Vector3.up, Vector3.forward, 360, aiSenses.viewRadius);



            Vector3 viewAngleA = aiSenses.DirFromAngle(-aiSenses.ViewAngle / 2, true);
            Vector3 viewAngleB = aiSenses.DirFromAngle(aiSenses.ViewAngle / 2, true);

            Handles.DrawLine(aiSenses.transform.position, aiSenses.transform.position + viewAngleA * aiSenses.viewRadius);
            Handles.DrawLine(aiSenses.transform.position, aiSenses.transform.position + viewAngleB * aiSenses.viewRadius);
            Handles.color = Color.red;


        }
    }
}
#endif  