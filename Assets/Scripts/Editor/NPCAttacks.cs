using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using IAUS.ECS2.Component;
using UnityEditor.AnimatedValues;

namespace IAUS.NPCSO.editor
{
    public sealed partial class NPCEditor : EditorWindow
    {
        List<AttackTypeInfo> GetAttacks;


      
        public List<AttackTypeInfo> SetupAttacks() {
            for (int i = 0; i < GetAttacks.Count; i++)
            {
                AttackTypeInfo temp = new AttackTypeInfo();
                temp = GetAttacks[i];
                EditorGUILayout.BeginVertical("Box");

                temp.style = (AttackStyle)EditorGUILayout.EnumPopup("Attack Type", temp.style);
                temp.AttackRange = (uint)EditorGUILayout.IntSlider("Distance to Target", (int)temp.AttackRange, 0, 100);
                temp.Attacktimer = EditorGUILayout.Slider("Distance to Target", temp.Attacktimer, 0, 45);

                EditorGUILayout.EndVertical();
                GetAttacks[i] = temp;
            }

            if (GUILayout.Button(("Add Attack")))
            {
                GetAttacks.Add(new AttackTypeInfo());
            }
            return GetAttacks;
        }
    }
}
