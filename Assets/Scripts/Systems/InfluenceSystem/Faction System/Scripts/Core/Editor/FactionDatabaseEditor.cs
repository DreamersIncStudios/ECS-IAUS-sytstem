#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.IO;
namespace DreamersInc.FactionSystem
{
    [CustomEditor(typeof(FactionDatabase),true)]
    public partial class FactionDatabaseEditor : Editor
    {
        private class FactionDatabaseFoldouts { 
            public bool factionsFoldout = false;
            public bool GenusTribeFoldout = false;
            public bool personalityTraitDefsFoldout = false;
            public bool relationshipTraitDefsFoldout = false;
            public bool InheritedGenusFoldout = false;
        }
        private static Dictionary<UnityEngine.Object, FactionDatabaseFoldouts> foldouts = new Dictionary<UnityEngine.Object, FactionDatabaseFoldouts>();
        private ReorderableList m_factionList;
        private ReorderableList genusTribeList;
        private ReorderableList m_factionTraitList;
        private ReorderableList genusTraitList;
        private ReorderableList m_personalityTraitDefList;
        private ReorderableList m_relationshipTraitDefList;
        private ReorderableList m_presetList;
        private ReorderableList m_presetTraitList;
        private ReorderableList m_factionRelationshipList;
        private ReorderableList m_factionRelationshipTraitList;
        private ReorderableList m_factionInheritedRelationshipsList;
        private int m_personalityTraitDefIndex;
        private int m_relationshipTraitDefIndex;
         private List<InheritedRelationship> m_inheritedRelationships;
        private void OnEnable()
        {
            if (target == null) return;
            SetupPersonalityTraitDefList();
            SetupRelationshipTraitDefList();
            SetupFactionList();
            SetupGenusTribeList();
        }
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            if (target == null) return;
            Undo.RecordObject(target, "FactionDatabase");
            DrawCustomUI();
        }
        private void DrawCustomUI() {
            CheckFoldouts();
            serializedObject.Update();
            DrawPersonalityTraitDefSection();
            DrawRelationshipTraitDefSection();
            DrawFactionsSection();
            DrawGenusTribeSection();
            serializedObject.ApplyModifiedProperties();


        }

        private void CheckFoldouts()
        {
            if (!foldouts.ContainsKey(target))
            {
                foldouts.Add(target, new FactionDatabaseFoldouts());
            }
        }

        private void DrawFactionsSection() {

            foldouts[target].factionsFoldout = EditorGUILayout.Foldout(foldouts[target].factionsFoldout, "Factions");
            if (foldouts[target].factionsFoldout)
            {
                m_factionList.DoLayoutList();
                if (m_factionTraitList == null)
                {
                    if (m_factionList.serializedProperty.arraySize > 0)
                    {
                        EditorGUILayout.HelpBox("Click the double bars to the left of a faction's name to edit it.", MessageType.None);
                    }
                }
                else
                {
                    DrawFactionEditSection();
                }
            }
        }

        private void DrawFactionEditSection()
        {
            EditorGUILayout.LabelField("Faction: " + m_factionList.serializedProperty.GetArrayElementAtIndex(m_factionList.index).FindPropertyRelative("name").stringValue);
            m_factionGenusTribeList.DoLayoutList();
            m_factionTraitList.DoLayoutList();
            m_factionRelationshipList.DoLayoutList();
            if (m_factionRelationshipTraitList != null)
            {
                m_factionRelationshipTraitList.DoLayoutList();
            }
            DrawInheritedRelationshipsSection();
           // DrawPercentJudgeParentsSection();
        }

        private void SetupFactionList() {
            m_factionList = new ReorderableList(
                serializedObject, serializedObject.FindProperty("factions"), true, true, true, true
                );
            m_factionList.drawHeaderCallback = OnDrawFactionListHeader;
            m_factionList.drawElementCallback = OnDrawFactionListElement;
            m_factionList.onAddCallback = OnAddFaction;
            m_factionList.onRemoveCallback = OnRemoveFaction;
            m_factionList.onSelectCallback = OnSelectFaction;

            SetupFactionEditList();
        }
        private void OnAddFaction(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            InitializeFactionElement(element, string.Empty, string.Empty);
            SetupFactionEditList(list.serializedProperty.GetArrayElementAtIndex(list.index));
        }

        private ReorderableList m_factionGenusTribeList;
        private void SetupFactionEditList(SerializedProperty faction)
        {

            m_factionRelationshipTraitList = null;
            m_factionInheritedRelationshipsList = null;
            m_inheritedRelationships = null;

            // Personality traits:
            m_factionTraitList = new ReorderableList(
                serializedObject, faction.FindPropertyRelative("traits"),
                false, true, true, false);
            m_factionTraitList.drawHeaderCallback = OnDrawFactionTraitListHeader;
            m_factionTraitList.drawElementCallback = OnDrawFactionTraitListElement;
          //  m_factionTraitList.onAddDropdownCallback = OnAddFactionTraitsDropdown;

            //// Parents:
            m_factionGenusTribeList = new ReorderableList(
                serializedObject, faction.FindPropertyRelative("genusTribes"),
                true, true, true, true);
            m_factionGenusTribeList.drawHeaderCallback = OnDrawFactionParentListHeader;
            m_factionGenusTribeList.drawElementCallback = OnDrawFactionParentListElement;
            m_factionGenusTribeList.onAddDropdownCallback = OnAddFactionParentDropdown;

            //// Relationships:
            m_factionRelationshipList = new ReorderableList(
                serializedObject, faction.FindPropertyRelative("relationships"),
                true, true, true, true);
            m_factionRelationshipList.drawHeaderCallback = OnDrawFactionRelationshipListHeader;
            m_factionRelationshipList.drawElementCallback = OnDrawFactionRelationshipListElement;
            m_factionRelationshipList.onAddDropdownCallback = OnAddFactionRelationshipDropdown;
            m_factionRelationshipList.onSelectCallback = OnSelectFactionRelationship;
            m_factionRelationshipList.onRemoveCallback = OnRemoveFactionRelationship;

     
        }

        private SerializedProperty FindFaction(int factionID)
        {
            for (int i = 0; i < m_factionList.serializedProperty.arraySize; i++)
            {
                var element = m_factionList.serializedProperty.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("id").intValue == factionID)
                {
                    return element;
                }
            }
            return null;
        }
        private SerializedProperty FindGenusTribe(int genusID)
        {
            for (int i = 0; i <genusTribeList.serializedProperty.arraySize; i++)
            {
                var element = genusTribeList .serializedProperty.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("id").intValue == genusID)
                {
                    return element;
                }
            }
            return null;
        }

        private void DrawInheritedRelationshipsSection()
        {
            EditorGUI.indentLevel++;
            foldouts[target].InheritedGenusFoldout = EditorGUILayout.Foldout(foldouts[target].InheritedGenusFoldout, "Inherited Relationships");
            if (foldouts[target].InheritedGenusFoldout)
            {
                if (m_inheritedRelationships == null || m_factionInheritedRelationshipsList == null)
                {
                    SetupInheritedRelationshipList();
                }
                m_factionInheritedRelationshipsList.DoLayoutList();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Refresh", GUILayout.Width(64)))
                {
                    SetupInheritedRelationshipList();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        private void InitializeFactionElement(SerializedProperty element, string name, string description)
        {
            var id = serializedObject.FindProperty("nextFactionID").intValue;
            serializedObject.FindProperty("nextFactionID").intValue++;
            element.FindPropertyRelative("id").intValue = id;
            element.FindPropertyRelative("name").stringValue = string.Empty;
            element.FindPropertyRelative("description").stringValue = string.Empty;
            var values = element.FindPropertyRelative("traits");
            values.arraySize = m_personalityTraitDefList.serializedProperty.arraySize;
            for (int j = 0; j < m_personalityTraitDefList.serializedProperty.arraySize; j++)
            {
                values.GetArrayElementAtIndex(j).floatValue = 0;
            }
            element.FindPropertyRelative("genusTribes").arraySize = 0;
            element.FindPropertyRelative("relationships").arraySize = 0;
        }

        private void OnRemoveFaction(ReorderableList list)
        {
            var factionName = list.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative("name").stringValue;
            if (EditorUtility.DisplayDialog("Delete selected faction?", factionName, "Delete", "Cancel"))
            {
                var faction = serializedObject.FindProperty("factions").GetArrayElementAtIndex(list.index);
                if (faction != null)
                {
                    var factionID = m_factionList.serializedProperty.GetArrayElementAtIndex(m_factionList.index).FindPropertyRelative("id").intValue;
                  //  NotifyFactionsRemoveFaction(factionID);
                }
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                m_factionTraitList = null;
            }
        }
        private void OnSelectFaction(ReorderableList list)
        {
            SetupFactionEditList(list.serializedProperty.GetArrayElementAtIndex(list.index));

        }

        private void SetupFactionEditList()
        {
            m_factionTraitList = null;
            m_factionGenusTribeList= null;
            m_factionRelationshipList = null;
            m_factionRelationshipTraitList = null;
        }
        private void OnDrawFactionListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Factions");
        }
        private void OnDrawFactionListElement(Rect rect, int index, bool isActive, bool isFocused) {
            var element = m_factionList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            var nameWidth = GetDefaultNameWidth(rect);
            var colorWidth = 56;
            var descriptionWidth = rect.width - nameWidth - colorWidth - 6;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, nameWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("name"), GUIContent.none);
            EditorGUI.PropertyField(
                new Rect(rect.x + rect.width - descriptionWidth - colorWidth - 2, rect.y, descriptionWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("description"), GUIContent.none);


        }
        private float GetDefaultNameWidth(Rect rect)
        {
            return Mathf.Clamp(rect.width / 4, 80, 200);
        }

        private void NotifyFactionsRemoveTrait(int index)
        {
            var factions = serializedObject.FindProperty("factions");
            for (int i = 0; i < factions.arraySize; i++)
            {
                var faction = factions.GetArrayElementAtIndex(i);
                faction.FindPropertyRelative("traits").DeleteArrayElementAtIndex(index);
            }
        }

    }
}
#endif