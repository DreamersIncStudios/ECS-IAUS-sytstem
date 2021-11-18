#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditorInternal;
using UnityEditor;

namespace DreamersInc.FactionSystem
{
    public partial class FactionDatabaseEditor : Editor
    {
        private void SetupInheritedRelationshipList()
        {
            m_inheritedRelationships = null;
            serializedObject.ApplyModifiedProperties();
            var factionID = m_factionList.serializedProperty.GetArrayElementAtIndex(m_factionList.index).FindPropertyRelative("id").intValue;
            m_inheritedRelationships = InheritedRelationship.GetInheritedRelationships(target as FactionDatabase, factionID);
            m_factionInheritedRelationshipsList = new ReorderableList(m_inheritedRelationships, typeof(Relationship), false, true, false, false);
            m_factionInheritedRelationshipsList.drawHeaderCallback = OnDrawInheritedRelationshipsListHeader;
            m_factionInheritedRelationshipsList.drawElementCallback = OnDrawInheritedRelationshipsListElement;
        }

        private void OnDrawInheritedRelationshipsListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Inherited Relationships");
        }


        private void OnDrawInheritedRelationshipsListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.width -= 16;
            rect.x += 16;
            rect.y += 2;
            var nameWidth = rect.width / 2;
            var valueWidth = rect.width - nameWidth - 4;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(
                new Rect(rect.x, rect.y, nameWidth, EditorGUIUtility.singleLineHeight),
                m_inheritedRelationships[index].name);
            EditorGUI.Slider(
                new Rect(rect.x + rect.width - valueWidth, rect.y, valueWidth, EditorGUIUtility.singleLineHeight),
                GUIContent.none, m_inheritedRelationships[index].affinity, -100, 100);
            EditorGUI.EndDisabledGroup();
        }
        private void OnDrawFactionTraitListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Personality Traits");
        }

        private void OnDrawFactionTraitListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = m_factionTraitList.serializedProperty.GetArrayElementAtIndex(index);
            DrawPersonalityTraitListElement(rect, index, isActive, isFocused, element);
        }

        private void DrawPersonalityTraitListElement(Rect rect, int index, bool isActive, bool isFocused, SerializedProperty element)
        {
            rect.width -= 16;
            rect.x += 16;
            rect.y += 2;
            var nameWidth = GetDefaultNameWidth(rect);
            var valueWidth = rect.width - nameWidth - 4;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(
                new Rect(rect.x, rect.y, nameWidth, EditorGUIUtility.singleLineHeight),
                m_personalityTraitDefList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("name").stringValue);
            EditorGUI.EndDisabledGroup();
            EditorGUI.Slider(
                new Rect(rect.x + rect.width - valueWidth, rect.y, valueWidth, EditorGUIUtility.singleLineHeight),
                element, -100, 100, GUIContent.none);
        }

        //private void OnAddFactionTraitsDropdown(Rect rect, ReorderableList list)
        //{
        //    var menu = new GenericMenu();
        //    var faction = m_factionList.serializedProperty.GetArrayElementAtIndex(m_factionList.index);
        //    var hasParents = (faction.FindPropertyRelative("genusTribes").arraySize > 0);
        //    if (hasParents)
        //    {
        //        menu.AddItem(new GUIContent("(Average from parents)"), false, OnSelectAverageTraitsMenuItem, faction.FindPropertyRelative("id").intValue);
        //        menu.AddItem(new GUIContent("(Sum from parents)"), false, OnSelectSumTraitsMenuItem, faction.FindPropertyRelative("id").intValue);
        //    }
        //    else
        //    {
        //        menu.AddDisabledItem(new GUIContent("(Average from parents)"));
        //        menu.AddDisabledItem(new GUIContent("(Sum from parents)"));
        //    }
        //    for (int i = 0; i < m_presetList.serializedProperty.arraySize; i++)
        //    {
        //        var preset = m_presetList.serializedProperty.GetArrayElementAtIndex(i);
        //        var presetName = preset.FindPropertyRelative("name").stringValue;
        //        menu.AddItem(
        //            new GUIContent(presetName),
        //            false, OnSelectFactionPresetMenuItem, preset);
        //    }
        //    menu.ShowAsContext();
        //}

        //private void OnSelectAverageTraitsMenuItem(object factionID)
        //{
        //    serializedObject.ApplyModifiedProperties();
        //    (target as FactionDatabase).InheritTraitsFromParents((int)factionID, FactionInheritanceType.Average);
        //    serializedObject.Update();
        //}

        //private void OnSelectSumTraitsMenuItem(object factionID)
        //{
        //    serializedObject.ApplyModifiedProperties();
        //    (target as FactionDatabase).InheritTraitsFromParents((int)factionID, FactionInheritanceType.Sum);
        //    serializedObject.Update();
        //}

        private void OnSelectFactionPresetMenuItem(object preset)
        {
            var presetTraits = (preset as SerializedProperty).FindPropertyRelative("traits");
            var faction = m_factionList.serializedProperty.GetArrayElementAtIndex(m_factionList.index);
            var factionTraits = faction.FindPropertyRelative("traits");
            for (int i = 0; i < presetTraits.arraySize; i++)
            {
                factionTraits.GetArrayElementAtIndex(i).floatValue = presetTraits.GetArrayElementAtIndex(i).floatValue;
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDrawFactionParentListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Parents");
        }

        private void OnDrawFactionParentListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var parentID = m_factionGenusTribeList.serializedProperty.GetArrayElementAtIndex(index).intValue;
            var parent = FindGenusTribe(parentID);
            if (parent == null) return;
            rect.y += 2;
            var m_nameWidth = Mathf.Clamp(rect.width / 4, 80, 200);
            var descriptionWidth = rect.width - m_nameWidth - 4;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, m_nameWidth, EditorGUIUtility.singleLineHeight),
                parent.FindPropertyRelative("name"), GUIContent.none);
            EditorGUI.PropertyField(
                new Rect(rect.x + rect.width - descriptionWidth, rect.y, descriptionWidth, EditorGUIUtility.singleLineHeight),
                parent.FindPropertyRelative("description"), GUIContent.none);
            EditorGUI.EndDisabledGroup();
        }

        private void OnAddFactionParentDropdown(Rect rect, ReorderableList list)
        {
            var menu = new GenericMenu();
            for (int i = 0; i < genusTribeList.serializedProperty.arraySize; i++)
            {
                    var genus = genusTribeList.serializedProperty.GetArrayElementAtIndex(i);
                    var genusID = genus.FindPropertyRelative("id").intValue;
                    var genusName = genus.FindPropertyRelative("name").stringValue;
                    menu.AddItem(
                        new GUIContent(genusName),
                        false, OnSelectFactionGenusTribeMenuItem, genusID);
            }
            menu.ShowAsContext();
        }
        private void OnDrawFactionRelationshipListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Relationships");
        }

        private void OnSelectFactionGenusTribeMenuItem(object tribeID)
        {
            var faction = m_factionList.serializedProperty.GetArrayElementAtIndex(m_factionList.index);
            var Genus = faction.FindPropertyRelative("genusTribes");
            Genus.arraySize++;
            Genus.GetArrayElementAtIndex(Genus.arraySize - 1).intValue = (int)tribeID;
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDrawFactionRelationshipListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var relationship = m_factionRelationshipList.serializedProperty.GetArrayElementAtIndex(index);
            var otherFaction = FindGenusTribe(relationship.FindPropertyRelative("factionID").intValue);
            if (otherFaction == null) return;
            rect.y += 2;
            var m_nameWidth = Mathf.Clamp(rect.width / 4, 80, 200);
            var valueWidth = rect.width - m_nameWidth - 18;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, m_nameWidth, EditorGUIUtility.singleLineHeight),
                otherFaction.FindPropertyRelative("name"), GUIContent.none);
            EditorGUI.EndDisabledGroup();
            var inheritable = relationship.FindPropertyRelative("inheritable");
            inheritable.boolValue = EditorGUI.Toggle(
                new Rect(rect.x + m_nameWidth + 2, rect.y, 16, EditorGUIUtility.singleLineHeight),
                new GUIContent(string.Empty, "Inheritable"), inheritable.boolValue);
            var traits = relationship.FindPropertyRelative("traits");
            if (traits.arraySize != m_relationshipTraitDefList.serializedProperty.arraySize) traits.arraySize = m_relationshipTraitDefList.serializedProperty.arraySize;
            EditorGUI.Slider(
                new Rect(rect.x + rect.width - valueWidth, rect.y, valueWidth, EditorGUIUtility.singleLineHeight),
                traits.GetArrayElementAtIndex(0), -100, 100, GUIContent.none);
        }

        private void OnAddFactionRelationshipDropdown(Rect rect, ReorderableList list)
        {
            var menu = new GenericMenu();
            for (int i = 0; i < m_factionList.serializedProperty.arraySize; i++)
            {
                var faction = m_factionList.serializedProperty.GetArrayElementAtIndex(i);
                var factionID = faction.FindPropertyRelative("id").intValue;
                var factionName = faction.FindPropertyRelative("name").stringValue;
                menu.AddItem(
                    new GUIContent(factionName),
                    false, OnSelectFactionRelationshipMenuItem, factionID);
            }
            menu.ShowAsContext();
        }

        private void OnSelectFactionRelationshipMenuItem(object factionID)
        {
            var faction = m_factionList.serializedProperty.GetArrayElementAtIndex(m_factionList.index);
            var relationships = faction.FindPropertyRelative("relationships");
            relationships.arraySize++;
            var relationship = relationships.GetArrayElementAtIndex(relationships.arraySize - 1);
            relationship.FindPropertyRelative("factionID").intValue = (int)factionID;
            relationship.FindPropertyRelative("inheritable").boolValue = true;
            var traits = relationship.FindPropertyRelative("traits");
            if (traits.arraySize != m_relationshipTraitDefList.serializedProperty.arraySize) traits.arraySize = m_relationshipTraitDefList.serializedProperty.arraySize;
            traits.GetArrayElementAtIndex(0).floatValue = 0;
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSelectFactionRelationship(ReorderableList list)
        {
            SetupFactionRelationshipTraitList(list.serializedProperty.GetArrayElementAtIndex(list.index));
        }


        private void SetupFactionRelationshipTraitList(SerializedProperty relationship)
        {
            m_factionRelationshipTraitList = new ReorderableList(
                serializedObject, relationship.FindPropertyRelative("traits"),
                false, true, false, false);
            m_factionRelationshipTraitList.drawHeaderCallback = OnDrawFactionRelationshipTraitListHeader;
            m_factionRelationshipTraitList.drawElementCallback = OnDrawFactionRelationshipTraitListElement;
        }

        private void OnRemoveFactionRelationship(ReorderableList list)
        {
            m_factionRelationshipTraitList = null;
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
        }
        private void OnDrawFactionRelationshipTraitListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Relationship Traits");
        }

        private void OnDrawFactionRelationshipTraitListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = m_factionRelationshipTraitList.serializedProperty.GetArrayElementAtIndex(index);
            DrawRelationshipTraitListElement(rect, index, isActive, isFocused, element);
        }

        private void DrawRelationshipTraitListElement(Rect rect, int index, bool isActive, bool isFocused, SerializedProperty element)
        {
            rect.width -= 16;
            rect.x += 16;
            rect.y += 2;
            var nameWidth = GetDefaultNameWidth(rect);
            var valueWidth = rect.width - nameWidth - 4;
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.TextField(
                new Rect(rect.x, rect.y, nameWidth, EditorGUIUtility.singleLineHeight),
                m_relationshipTraitDefList.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("name").stringValue);
            EditorGUI.EndDisabledGroup();
            EditorGUI.Slider(
                new Rect(rect.x + rect.width - valueWidth, rect.y, valueWidth, EditorGUIUtility.singleLineHeight),
                element, -100, 100, GUIContent.none);
        }
    }
}
#endif