#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.IO;
namespace DreamersInc.FactionSystem
{
    public partial class FactionDatabaseEditor : Editor
    {
        private void DrawGenusTribeSection()
        {

            foldouts[target].GenusTribeFoldout = EditorGUILayout.Foldout(foldouts[target].GenusTribeFoldout, "Genus/Tribe");
            if (foldouts[target].GenusTribeFoldout)
            {
                genusTribeList.DoLayoutList();
                if (genusTraitList == null)
                {
                    if (genusTribeList.serializedProperty.arraySize > 0)
                    {
                        EditorGUILayout.HelpBox("Click the double bars to the left of a faction's name to edit it.", MessageType.None);
                    }
                }
                else
                {
                    DrawGenusTribeEditSection();
                }
            }
        }


        private void DrawGenusTribeEditSection()
        {
            EditorGUILayout.LabelField("Genus/Tribe: " + genusTribeList.serializedProperty.GetArrayElementAtIndex(genusTribeList.index).FindPropertyRelative("name").stringValue);
            GenusRelationships.DoLayoutList();
             if (m_factionRelationshipTraitList != null)
             {
                 m_factionRelationshipTraitList.DoLayoutList();
             }
    
           //  DrawPercentJudgeParentsSection();
        }
        private void SetupGenusTribeList()
        {
            genusTribeList = new ReorderableList(
                serializedObject, serializedObject.FindProperty("genusTribes"), true, true, true, true
                );
            genusTribeList.drawHeaderCallback = OnDrawGenusListHeader;
            genusTribeList.drawElementCallback = OnDrawGenusListElement;
            genusTribeList.onAddCallback = OnAddGenus;
            genusTribeList.onRemoveCallback = OnRemoveGenus;
            genusTribeList.onSelectCallback = OnSelectGenus;

            SetupGenusTribeEditList();
        }

        private void OnAddGenus(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
             InitializeGenusElement(element, string.Empty, string.Empty);
              SetupGenusTribeEditList(list.serializedProperty.GetArrayElementAtIndex(list.index));


        }
        private void InitializeGenusElement(SerializedProperty element, string name, string description)
        {
            var id = serializedObject.FindProperty("nextGenusTribeID").intValue;
            serializedObject.FindProperty("nextGenusTribeID").intValue++;
            element.FindPropertyRelative("id").intValue = id;
            element.FindPropertyRelative("name").stringValue = string.Empty;
            element.FindPropertyRelative("description").stringValue = string.Empty;
            var values = element.FindPropertyRelative("traits");
            values.arraySize = m_personalityTraitDefList.serializedProperty.arraySize;
            for (int j = 0; j < m_personalityTraitDefList.serializedProperty.arraySize; j++)
            {
                values.GetArrayElementAtIndex(j).floatValue = 0;
            }
            //element.FindPropertyRelative("genusTribes").arraySize = 0;
            element.FindPropertyRelative("relationships").arraySize = 0;
        }




        private void OnDrawGenusListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Genus/Tribe");
        }
        private void OnDrawGenusListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = genusTribeList.serializedProperty.GetArrayElementAtIndex(index);
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

        private void OnRemoveGenus(ReorderableList list)
        {
            var genusName = list.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative("name").stringValue;
            if (EditorUtility.DisplayDialog("Delete selected faction?", genusName, "Delete", "Cancel"))
            {
                var genus = serializedObject.FindProperty("genusTribes").GetArrayElementAtIndex(list.index);
                if (genus != null)
                {
                    var genusID = m_factionList.serializedProperty.GetArrayElementAtIndex(genusTribeList.index).FindPropertyRelative("id").intValue;
                    //  NotifyFactionsRemoveFaction(factionID);
                }
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
                genusTraitList = null;


            }
        }
        private void OnSelectGenus(ReorderableList list)
        {
            SetupGenusTribeEditList(list.serializedProperty.GetArrayElementAtIndex(list.index));
           
        }
        private void SetupGenusTribeEditList()
        {
            genusTraitList = null;
            GenusRelationships = null;
        }
        private ReorderableList GenusRelationships;
        private void SetupGenusTribeEditList(SerializedProperty genusTribe)
        {

            genusTraitList = new ReorderableList(
                serializedObject, genusTribe.FindPropertyRelative("traits"),
                false, true, true, false);

            GenusRelationships = new ReorderableList(
                serializedObject, genusTribe.FindPropertyRelative("relationships"),
                true, true, true, true);
            GenusRelationships.drawHeaderCallback = OnDrawGenusTribeListHeader;
            GenusRelationships.drawElementCallback = OnDrawParentListElement;
           GenusRelationships.onAddDropdownCallback= OnAddGenusTribeRelationDropdown;
      

        }

        private void OnDrawGenusTribeListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Genus and Tribes Relations");
        }

        private void OnDrawParentListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var relationship = GenusRelationships.serializedProperty.GetArrayElementAtIndex(index);
            var parentID= relationship.FindPropertyRelative("factionID").intValue;
            var parent = FindGenusTribe(parentID);
            if (parent == null) return;
            rect.y += 2;
            var m_nameWidth = Mathf.Clamp(rect.width / 4, 80, 200);
            var valueWidth = rect.width - m_nameWidth - 18;
            var descriptionWidth = rect.width - m_nameWidth - 4;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, m_nameWidth, EditorGUIUtility.singleLineHeight),
                parent.FindPropertyRelative("name"), GUIContent.none);
            var traits = relationship.FindPropertyRelative("traits");

            EditorGUI.EndDisabledGroup();
            EditorGUI.Slider(
               new Rect(rect.x + rect.width - valueWidth, rect.y, valueWidth, EditorGUIUtility.singleLineHeight),
               traits.GetArrayElementAtIndex(0), -100, 100, GUIContent.none);
        }

        private void OnAddGenusTribeRelationDropdown(Rect rect, ReorderableList list) {
            var menu = new GenericMenu();
            for (int i = 0; i < genusTribeList.serializedProperty.arraySize; i++)
            {
                if (i != genusTribeList.index)
                {
                    var genus = genusTribeList.serializedProperty.GetArrayElementAtIndex(i);
                    var genusID = genus.FindPropertyRelative("id").intValue;
                    var genusName = genus.FindPropertyRelative("name").stringValue;
                    menu.AddItem(
                        new GUIContent(genusName),
                        false, OnSelectGenusTribeMenuItem, genusID);
                }
            }
            menu.ShowAsContext();
        }
        private void OnSelectGenusTribeMenuItem(object tribeID)
        {
            var faction = genusTribeList.serializedProperty.GetArrayElementAtIndex(genusTribeList.index);
            var relationships = faction.FindPropertyRelative("relationships");
            relationships.arraySize++;
            var relationship = relationships.GetArrayElementAtIndex(relationships.arraySize - 1);
            relationship.FindPropertyRelative("factionID").intValue = (int)tribeID;
            relationship.FindPropertyRelative("inheritable").boolValue = true;
            var traits = relationship.FindPropertyRelative("traits");
            traits.arraySize++;
            traits.GetArrayElementAtIndex(0).floatValue = 0;
            serializedObject.ApplyModifiedProperties();
        }

    }
}
#endif