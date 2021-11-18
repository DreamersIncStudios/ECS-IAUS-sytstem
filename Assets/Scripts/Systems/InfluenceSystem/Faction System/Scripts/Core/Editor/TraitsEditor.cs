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
        private void SetupPersonalityTraitDefList()
        {
            m_personalityTraitDefList = new ReorderableList(
                serializedObject, serializedObject.FindProperty("personalityTraitDefinitions"),
                true, true, true, true);
            m_personalityTraitDefList.drawHeaderCallback = OnDrawPersonalityTraitDefListHeader;
            m_personalityTraitDefList.drawElementCallback = OnDrawPersonalityTraitDefListElement;
            m_personalityTraitDefList.onAddCallback = OnAddPersonalityTraitDef;
            m_personalityTraitDefList.onRemoveCallback = OnRemovePersonalityTraitDef;
            m_personalityTraitDefList.onSelectCallback = OnSelectPersonalityTraitDef;
            m_personalityTraitDefList.onReorderCallback = OnReorderPersonalityTraitDef;
        }


        private void SetupRelationshipTraitDefList()
        {
            m_relationshipTraitDefList = new ReorderableList(
                serializedObject, serializedObject.FindProperty("relationshipTraitDefinitions"),
                true, true, true, true);
            m_relationshipTraitDefList.drawHeaderCallback = OnDrawRelationshipTraitDefListHeader;
            m_relationshipTraitDefList.drawElementCallback = OnDrawRelationshipTraitDefListElement;
            m_relationshipTraitDefList.onAddCallback = OnAddRelationshipTraitDef;
            m_relationshipTraitDefList.onRemoveCallback = OnRemoveRelationshipTraitDef;
            m_relationshipTraitDefList.onSelectCallback = OnSelectRelationshipTraitDef;
            m_relationshipTraitDefList.onReorderCallback = OnReorderRelationshipTraitDef;
        }

        #region Personality traits 
        private void DrawPersonalityTraitDefSection()
        {
            foldouts[target].personalityTraitDefsFoldout = EditorGUILayout.Foldout(foldouts[target].personalityTraitDefsFoldout, "Personality Traits");
            if (foldouts[target].personalityTraitDefsFoldout)
            {
                m_personalityTraitDefList.DoLayoutList();
              //  EditorGUILayout.PropertyField(serializedObject.FindProperty("traitInheritanceType"));
            }
        }
        private void OnDrawPersonalityTraitDefListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Personality Traits");
        }
        private void OnDrawPersonalityTraitDefListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_personalityTraitDefList != null && 0 <= index && index <= m_personalityTraitDefList.serializedProperty.arraySize)
            {
                var element = m_personalityTraitDefList.serializedProperty.GetArrayElementAtIndex(index);
                DrawNameDescriptionListElement(rect, index, isActive, isFocused, true, element);
            }
        }
        private void DrawNameDescriptionListElement(Rect rect, int index, bool isActive, bool isFocused, bool isEditable, SerializedProperty element)
        {
            rect.y += 2;
            var nameWidth = GetDefaultNameWidth(rect);
            var descriptionWidth = rect.width - nameWidth - 4;
            if (isEditable)
            {
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, nameWidth, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("name"), GUIContent.none);
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(
                    new Rect(rect.x, rect.y, nameWidth, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("name").stringValue);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.PropertyField(
                new Rect(rect.x + rect.width - descriptionWidth, rect.y, descriptionWidth, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative("description"), GUIContent.none);
        }
        private void OnAddPersonalityTraitDef(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("name").stringValue = string.Empty;
            element.FindPropertyRelative("description").stringValue = string.Empty;
            NotifyPresetsAddTrait();
            NotifyFactionsAddTrait();
        }

        private void OnRemovePersonalityTraitDef(ReorderableList list)
        {
            var traitName = list.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative("name").stringValue;
            if (EditorUtility.DisplayDialog("Delete selected personality trait?", traitName, "Delete", "Cancel"))
            {
                NotifyPresetsRemoveTrait(list.index);
                NotifyFactionsRemoveTrait(list.index);
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            }
        }

        private void OnSelectPersonalityTraitDef(ReorderableList list)
        {
            m_personalityTraitDefIndex = list.index;
        }

        private void OnReorderPersonalityTraitDef(ReorderableList list)
        {
            NotifyPresetsReorderTrait(m_personalityTraitDefIndex, list.index);
            NotifyFactionsReorderTrait(m_personalityTraitDefIndex, list.index);
        }

        private void NotifyFactionsAddTrait()
        {
            var traits = serializedObject.FindProperty("personalityTraitDefinitions");
            var factions = serializedObject.FindProperty("factions");
            for (int i = 0; i < factions.arraySize; i++)
            {
                var faction = factions.GetArrayElementAtIndex(i);
                faction.FindPropertyRelative("traits").arraySize = traits.arraySize;
            }
        }

        private void NotifyPresetsRemoveTrait(int index)
        {
            var presets = serializedObject.FindProperty("presets");
            for (int i = 0; i < presets.arraySize; i++)
            {
                var preset = presets.GetArrayElementAtIndex(i);
                preset.FindPropertyRelative("traits").DeleteArrayElementAtIndex(index);
            }
        }

        private void NotifyPresetsAddTrait()
        {
            var traitDefs = serializedObject.FindProperty("personalityTraitDefinitions");
            var presets = serializedObject.FindProperty("presets");
            for (int i = 0; i < presets.arraySize; i++)
            {
                var preset = presets.GetArrayElementAtIndex(i);
                preset.FindPropertyRelative("traits").arraySize = traitDefs.arraySize;
            }
        }

        private void NotifyPresetsReorderTrait(int oldIndex, int newIndex)
        {
            var presets = serializedObject.FindProperty("presets");
            for (int i = 0; i < presets.arraySize; i++)
            {
                var preset = presets.GetArrayElementAtIndex(i);
                var values = preset.FindPropertyRelative("traits");
                var value = values.GetArrayElementAtIndex(oldIndex).floatValue;
                values.DeleteArrayElementAtIndex(oldIndex);
                values.InsertArrayElementAtIndex(newIndex);
                values.GetArrayElementAtIndex(newIndex).floatValue = value;
            }
        }

        private void NotifyFactionsReorderTrait(int oldIndex, int newIndex)
        {
            var factions = serializedObject.FindProperty("factions");
            for (int i = 0; i < factions.arraySize; i++)
            {
                var faction = factions.GetArrayElementAtIndex(i);
                var values = faction.FindPropertyRelative("traits");
                var value = values.GetArrayElementAtIndex(oldIndex).floatValue;
                values.DeleteArrayElementAtIndex(oldIndex);
                values.InsertArrayElementAtIndex(newIndex);
                values.GetArrayElementAtIndex(newIndex).floatValue = value;
            }
        }

        #endregion
        #region RelationshipTraits
        private const string AffinityTraitName = "Affinity";

        private void DrawRelationshipTraitDefSection()
        {
            foldouts[target].relationshipTraitDefsFoldout = EditorGUILayout.Foldout(foldouts[target].relationshipTraitDefsFoldout, "Relationship Traits");
            if (foldouts[target].relationshipTraitDefsFoldout)
            {
                m_relationshipTraitDefList.DoLayoutList();
//                DrawRelationshipInheritanceTypeDropdown();
            }
        }


        private void OnDrawRelationshipTraitDefListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Relationship Traits");
        }

        private void OnDrawRelationshipTraitDefListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = m_relationshipTraitDefList.serializedProperty.GetArrayElementAtIndex(index);
            DrawNameDescriptionListElement(rect, index, isActive, isFocused, index > 0, element);
        }

        private void OnAddRelationshipTraitDef(ReorderableList list)
        {
            var index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("name").stringValue = string.Empty;
            element.FindPropertyRelative("description").stringValue = string.Empty;
            NotifyRelationshipsAddTrait();
        }

        private void OnRemoveRelationshipTraitDef(ReorderableList list)
        {
            var traitName = list.serializedProperty.GetArrayElementAtIndex(list.index).FindPropertyRelative("name").stringValue;
            if (string.Equals(traitName, AffinityTraitName)) return;
            if (EditorUtility.DisplayDialog("Delete selected relationship trait?", traitName, "Delete", "Cancel"))
            {
                NotifyRelationshipsRemoveTrait(list.index);
                ReorderableList.defaultBehaviours.DoRemoveButton(list);
            }
        }

        private void OnSelectRelationshipTraitDef(ReorderableList list)
        {
            m_relationshipTraitDefIndex = list.index;
        }

        private void OnReorderRelationshipTraitDef(ReorderableList list)
        {
            if (list.serializedProperty.arraySize < 1) return;
            NotifyRelationshipsReorderTrait(m_relationshipTraitDefIndex, list.index);

            // Make sure Affinity is always at top:
            var firstName = list.serializedProperty.GetArrayElementAtIndex(0).FindPropertyRelative("name").stringValue;
            if (!string.Equals(firstName, AffinityTraitName))
            {
                int affinityIndex = -1;
                string affinityDescription = string.Empty;
                for (int i = 0; i < list.serializedProperty.arraySize; i++)
                {
                    var iName = list.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue;
                    if (string.Equals(iName, AffinityTraitName))
                    {
                        affinityIndex = i;
                        affinityDescription = list.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("description").stringValue;
                        break;
                    }
                }
                if (affinityIndex != -1)
                {
                    list.serializedProperty.DeleteArrayElementAtIndex(affinityIndex);
                    list.serializedProperty.InsertArrayElementAtIndex(0);
                    list.serializedProperty.GetArrayElementAtIndex(0).FindPropertyRelative("name").stringValue = AffinityTraitName;
                    list.serializedProperty.GetArrayElementAtIndex(0).FindPropertyRelative("description").stringValue = affinityDescription;
                    NotifyRelationshipsReorderTrait(affinityIndex, 0);
                }
            }
        }

        private void NotifyRelationshipsAddTrait()
        {
            var traits = serializedObject.FindProperty("relationshipTraitDefinitions");
            var factions = serializedObject.FindProperty("factions");
            for (int factionIndex = 0; factionIndex < factions.arraySize; factionIndex++)
            {
                var faction = factions.GetArrayElementAtIndex(factionIndex);
                var relationships = faction.FindPropertyRelative("relationships");
                for (int relationshipIndex = 0; relationshipIndex < relationships.arraySize; relationshipIndex++)
                {
                    var relationship = relationships.GetArrayElementAtIndex(relationshipIndex);
                    relationship.FindPropertyRelative("traits").arraySize = traits.arraySize;
                }
            }
        }

        private void NotifyRelationshipsRemoveTrait(int index)
        {
            var factions = serializedObject.FindProperty("factions");
            for (int factionIndex = 0; factionIndex < factions.arraySize; factionIndex++)
            {
                var faction = factions.GetArrayElementAtIndex(factionIndex);
                var relationships = faction.FindPropertyRelative("relationships");
                for (int relationshipIndex = 0; relationshipIndex < relationships.arraySize; relationshipIndex++)
                {
                    var relationship = relationships.GetArrayElementAtIndex(relationshipIndex);
                    relationship.FindPropertyRelative("traits").DeleteArrayElementAtIndex(index);
                }
            }
        }

        private void NotifyRelationshipsReorderTrait(int oldIndex, int newIndex)
        {
            var factions = serializedObject.FindProperty("factions");
            for (int factionIndex = 0; factionIndex < factions.arraySize; factionIndex++)
            {
                var faction = factions.GetArrayElementAtIndex(factionIndex);
                var relationships = faction.FindPropertyRelative("relationships");
                for (int relationshipIndex = 0; relationshipIndex < relationships.arraySize; relationshipIndex++)
                {
                    var relationship = relationships.GetArrayElementAtIndex(relationshipIndex);
                    var values = relationship.FindPropertyRelative("traits");
                    var value = values.GetArrayElementAtIndex(oldIndex).floatValue;
                    values.DeleteArrayElementAtIndex(oldIndex);
                    values.InsertArrayElementAtIndex(newIndex);
                    values.GetArrayElementAtIndex(newIndex).floatValue = value;
                }
            }
        }

        #endregion
    }
}
#endif