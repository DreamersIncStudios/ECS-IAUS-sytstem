using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using IAUS.ECS.Component;
using IAUS.ECS;
using IAUS.ECS.Consideration;
namespace IAUS.NPCSO.editor
{
    public sealed partial class NPCEditor : EditorWindow
    {
        List<AttackTypeInfo> GetAttacks = new List<AttackTypeInfo>();


      
        public List<AttackTypeInfo> SetupAttacks() {
            for (int i = 0; i < GetAttacks.Count; i++)
            {
                AttackTypeInfo temp;
                temp = GetAttacks[i];
                EditorGUILayout.BeginVertical("Box");

                temp.style = (AttackStyle)EditorGUILayout.EnumPopup("Attack Type", temp.style);
                temp.AttackRange = (uint)EditorGUILayout.IntSlider("Distance to Target", (int)temp.AttackRange, 0, 100);
                temp.Attacktimer = EditorGUILayout.Slider("Time between attacks", temp.Attacktimer, 0, 45);
                if (temp.Range = EditorGUILayout.Foldout(temp.Range, "Attack Range Consideration"))
                    temp.RangeToTarget = DisplayConsideration(temp.RangeToTarget);
                if (temp.ManaAmmo = EditorGUILayout.Foldout(temp.ManaAmmo, "Mana/Ammo Amount Consideration"))
                    temp.ManaAmmoAmount = DisplayConsideration(temp.ManaAmmoAmount);

                GetAttacks[i] = temp;
                if (GUILayout.Button(("Remove Attack")))
                {
                    GetAttacks.Remove(temp);
                }
                EditorGUILayout.EndVertical();

            }

            if (GUILayout.Button(("Add Attack")))
            {
                GetAttacks.Add(new AttackTypeInfo() { 
                    HealthRatio = new ConsiderationScoringData()
                    {
                           M = 50, K = -0.95f, B = .935f, C = .35f, responseType = ResponseType.Logistic },
           
                    RangeToTarget = new ConsiderationScoringData()
                    {
                        M = 50,
                        K = -0.95f,
                        B = .935f,
                        C = .35f,
                        responseType = ResponseType.Logistic
                    },
                    
                    ManaAmmoAmount = new ConsiderationScoringData()
                    {
                        M = 50,
                        K = -0.95f,
                        B = .935f,
                        C = .35f,
                        responseType = ResponseType.Logistic
                    },

                }

                );
            }
            return GetAttacks;
        }
    }
}
