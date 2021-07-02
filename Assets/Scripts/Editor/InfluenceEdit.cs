using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using InfluenceSystem.Component;
namespace IAUS.NPCSO.editor
{
    public sealed partial class NPCEditor : EditorWindow
    {
        Influence GetInfluence;

        Influence SetupInfluence() {
            
            GetInfluence.faction = (Faction)EditorGUILayout.EnumPopup("Faction", GetInfluence.faction);
            GetInfluence.Level = (NPCLevel)EditorGUILayout.EnumPopup("Rank", GetInfluence.Level);

            GetInfluence.InfluenceValue = (uint)EditorGUILayout.IntSlider("Influence", (int)GetInfluence.InfluenceValue, 0, 20);
            GetInfluence.Range = (uint)EditorGUILayout.IntSlider("Range",(int)GetInfluence.Range, 0, 50);



            return GetInfluence;
        }
    }
}
