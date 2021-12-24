// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.LoveHate
{

    public delegate void PersonalityTraitChangedDelegate(int factionID, int traitID, float value);

    /// <summary>
    /// A faction database is a collection of traits, presets, and factions. The scope of a
    /// faction database is the definition of traits, presets, and factions, with no 
    /// awareness of faction members or other scene-level objects.
    /// </summary>
    public class FactionDatabase : ScriptableObject
    {

        /// <summary>
        /// Faction ID 0 is reserved for the player.
        /// </summary>
        public const int PlayerFactionID = 0;

        /// <summary>
        /// The personality trait definitions. Used for factions, deeds, and presets.
        /// </summary>
        public TraitDefinition[] personalityTraitDefinitions = new TraitDefinition[0];

        /// <summary>
        /// The relationship trait definitions. Used for relationships.
        /// </summary>
        public TraitDefinition[] relationshipTraitDefinitions = new TraitDefinition[1] { new TraitDefinition("Affinity", "(Required)") };

        /// <summary>
        /// The presets.
        /// </summary>
        public Preset[] presets = new Preset[0];

        /// <summary>
        /// The factions.
        /// </summary>
        public Faction[] factions = new Faction[0];

        /// <summary>
        /// How traits are inherited from parents.
        /// </summary>
        public FactionInheritanceType traitInheritanceType = FactionInheritanceType.Average;

        /// <summary>
        /// How relationships are inherited from parents.
        /// </summary>
        public FactionInheritanceType relationshipInheritanceType = FactionInheritanceType.Average;

        /// <summary>
        /// The next faction ID, used by the custom editor to auto-number factions.
        /// </summary>
        public int nextID = 0;

        private Dictionary<int, Faction> m_factionIDLookup = new Dictionary<int, Faction>();

        private Dictionary<string, Faction> m_factionNameLookup = new Dictionary<string, Faction>();

        public event PersonalityTraitChangedDelegate personalityTraitChanged = delegate { };

        public void Initialize()
        {
            personalityTraitDefinitions = new TraitDefinition[0];
            relationshipTraitDefinitions = new TraitDefinition[1] { new TraitDefinition("Affinity", "(Required)") };
            presets = new Preset[0];
            factions = new Faction[1] { new Faction(PlayerFactionID, "Player") };
            nextID = PlayerFactionID + 1;
        }

        private void SetFactionIDLookup(int factionID, Faction faction)
        {
            if (m_factionIDLookup.ContainsKey(factionID))
            {
                m_factionIDLookup[factionID] = faction;
            }
            else
            {
                m_factionIDLookup.Add(factionID, faction);
            }
        }
        public bool IsPlaying;
        private Faction LookupFactionByID(int factionID)
        {
            if (IsPlaying && m_factionIDLookup.ContainsKey(factionID))
            {
                return m_factionIDLookup[factionID];
            }
            else
            {
                for (int f = 0; f < factions.Length; f++)
                {
                    var faction = factions[f];
                    if ((faction != null) && (faction.id == factionID))
                    {
                        if (IsPlaying) m_factionIDLookup.Add(factionID, faction);
                        return faction;
                    }
                }
                return null;
            }
        }

        private void SetFactionNameLookup(string factionName, Faction faction)
        {
            if (m_factionNameLookup.ContainsKey(factionName))
            {
                m_factionNameLookup[factionName] = faction;
            }
            else
            {
                m_factionNameLookup.Add(factionName, faction);
            }
        }

        private Faction LookupFactionByName(string factionName)
        {
            if (Application.isPlaying && m_factionNameLookup.ContainsKey(factionName))
            {
                return m_factionNameLookup[factionName];
            }
            else
            {
                for (int f = 0; f < factions.Length; f++)
                {
                    var faction = factions[f];
                    if ((faction != null) && string.Equals(faction.name, factionName))
                    {
                        if (Application.isPlaying) m_factionNameLookup.Add(factionName, faction);
                        return faction;
                    }
                }
                return null;
            }
        }

        private int LookupFactionIDByName(string factionName)
        {
            var faction = LookupFactionByName(factionName);
            return (faction != null) ? faction.id : -1;
        }

        private void SetFactionLookups(Faction faction)
        {
            SetFactionIDLookup(faction.id, faction);
            SetFactionNameLookup(faction.name, faction);
        }

        private void RemoveFactionLookups(Faction faction)
        {
            if (m_factionIDLookup.ContainsKey(faction.id))
            {
                m_factionIDLookup.Remove(faction.id);
            }
            if (m_factionNameLookup.ContainsKey(faction.name))
            {
                m_factionNameLookup.Remove(faction.name);
            }
        }

        /// <summary>
        /// Creates a new faction in the faction database.
        /// </summary>
        /// <returns>The new faction's faction ID.</returns>
        /// <param name="factionName">Faction name.</param>
        public int CreateNewFaction(string factionName, string description)
        {
            var newFaction = new Faction(nextID++, factionName);
            newFaction.description = description;
            newFaction.traits = new float[personalityTraitDefinitions.Length];
            for (int i = 0; i < newFaction.traits.Length; i++)
            {
                newFaction.traits[i] = 0;
            }
            List<Faction> factionList = new List<Faction>(factions);
            factionList.Add(newFaction);
            factions = factionList.ToArray();
            SetFactionLookups(newFaction);
            return newFaction.id;
        }

        /// <summary>
        /// Permanently removes a faction from the faction database.
        /// </summary>
        /// <param name="factionName">Faction name.</param>
        public void DestroyFaction(string factionName)
        {
            DestroyFaction(GetFactionID(factionName));
        }

        /// <summary>
        /// Permanently removes a faction from the faction database.
        /// </summary>
        /// <param name="factionID">Faction ID.</param>
        public void DestroyFaction(int factionID)
        {
            var faction = GetFaction(factionID);
            if (faction == null) return;
            for (int f = 0; f < factions.Length; f++)
            {
                if (factions[f].id == factionID) continue;
                var other = factions[f];
                other.RemovePersonalRelationship(factionID);
                other.RemoveDirectParent(factionID);
            }
            var list = new List<Faction>(factions);
            list.Remove(faction);
            RemoveFactionLookups(faction);
            factions = list.ToArray();
        }

        /// <summary>
        /// Gets the faction with the specified faction ID.
        /// </summary>
        /// <returns>The faction, or `null` if no faction exists with the ID.</returns>
        /// <param name="factionID">Faction ID.</param>
        public Faction GetFaction(int factionID)
        {
            return LookupFactionByID(factionID);
        }

        /// <summary>
        /// Gets the faction with the specified name.
        /// </summary>
        /// <returns>The faction, or `null` if no faction exists by that name.</returns>
        /// <param name="factionName">Faction name.</param>
        public Faction GetFaction(string factionName)
        {
            return LookupFactionByName(factionName);
        }

        /// <summary>
        /// Gets a faction's ID from its name.
        /// </summary>
        /// <returns>The faction ID, or `-1` if no faction exists by that name.</returns>
        /// <param name="factionName">Faction name.</param>
        public int GetFactionID(string factionName)
        {
            return LookupFactionIDByName(factionName);
        }

        /// <summary>
        /// Determines whether a faction has another faction as its parent, grandparent, etc.
        /// </summary>
        /// <returns><c>true</c>, if has ancestor, <c>false</c> otherwise.</returns>
        /// <param name="factionID">Faction ID.</param>
        /// <param name="ancestorID">Ancestor ID.</param>
        public bool FactionHasAncestor(int factionID, int ancestorID)
        {
            return FactionHasAncestorRecursive(factionID, ancestorID, new List<int>(), 0);
        }

        /// <summary>
        /// The max recursion depth, used to prevent infinite loops.
        /// </summary>
        private const int MaxRecursionDepth = 128;

        private bool FactionHasAncestorRecursive(int factionID, int ancestorID, List<int> visited, int depth)
        {
            if (depth > MaxRecursionDepth) return false;
            if (visited.Contains(factionID)) return false;
            visited.Add(factionID);
            var faction = GetFaction(factionID);
            if (faction == null) return false;
            if (faction.HasDirectParent(ancestorID)) return true;
            for (int p = 0; p < faction.parents.Length; p++)
            {
                var parentID = faction.parents[p];
                if (FactionHasAncestorRecursive(parentID, ancestorID, visited, depth + 1)) return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether a faction has another faction as one of its direct parents.
        /// </summary>
        /// <returns><c>true</c>, if has direct parent, <c>false</c> otherwise.</returns>
        /// <param name="factionID">Faction ID.</param>
        /// <param name="parentID">Parent ID.</param>
        public bool FactionHasDirectParent(int factionID, int parentID)
        {
            var faction = GetFaction(factionID);
            return (faction == null) ? false : faction.HasDirectParent(parentID);
        }

        /// <summary>
        /// Adds a direct parent to a faction.
        /// </summary>
        /// <param name="factionID">Faction ID.</param>
        /// <param name="parentID">Parent ID.</param>
        public void AddFactionParent(int factionID, int parentID)
        {
            var faction = GetFaction(factionID);
            if (faction == null) return;
            faction.AddDirectParent(parentID);
        }

        /// <summary>
        /// Removes a direction parent from a faction.
        /// </summary>
        /// <param name="factionID">Faction ID.</param>
        /// <param name="parentID">Parent ID.</param>
        /// <param name="inheritRelationships">If set to <c>true</c> inherit any of the parents' relationships for which the faction doesn't already have a personal relationship.</param>
        public void RemoveFactionParent(int factionID, int parentID, bool inheritRelationships)
        {
            var faction = GetFaction(factionID);
            var parent = GetFaction(parentID);
            if (faction == null || parent == null || !faction.HasDirectParent(parentID)) return;
            faction.RemoveDirectParent(parentID);
            if (inheritRelationships)
            {
                for (int r = 0; r < parent.relationships.Count; r++)
                {
                    var relationship = parent.relationships[r];
                    if (!faction.HasPersonalRelationship(relationship.factionID))
                    {
                        faction.relationships.Add(Relationship.GetNew(relationship.factionID, (float[])relationship.traits.Clone()));
                    }
                }
            }
        }

        /// <summary>
        /// Gets a personality trait ID by its name.
        /// </summary>
        /// <returns>The personality trait ID.</returns>
        /// <param name="traitName">Trait name.</param>
        public int GetPersonalityTraitID(string traitName)
        {
            for (int i = 0; i < personalityTraitDefinitions.Length; i++)
            {
                if (string.Equals(traitName, personalityTraitDefinitions[i].name)) return i;
            }
            return -1;
        }

        /// <summary>
        /// Gets a faction's personality trait value.
        /// </summary>
        /// <param name="factionName">Faction.</param>
        /// <param name="traitID">Trait ID.</param>
        /// <returns></returns>
        public float GetPersonalityTrait(string factionName, int traitID)
        {
            return GetPersonalityTrait(GetFactionID(factionName), traitID);
        }

        /// <summary>
        /// Gets a faction's personality trait value.
        /// </summary>
        /// <param name="factionID">Faction.</param>
        /// <param name="traitID">Trait ID.</param>
        /// <returns></returns>
        public float GetPersonalityTrait(int factionID, int traitID)
        {
            var faction = GetFaction(factionID);
            return (faction != null && (0 <= traitID && traitID < faction.traits.Length)) ? faction.traits[traitID] : 0;
        }

        /// <summary>
        /// Sets a faction's personality trait value.
        /// </summary>
        /// <param name="factionName">Faction.</param>
        /// <param name="traitID">Trait ID.</param>
        /// <param name="value">New value.</param>
        public void SetPersonalityTrait(string factionName, int traitID, float value)
        {
            SetPersonalityTrait(GetFactionID(factionName), traitID, value);
        }

        /// <summary>
        /// Sets a faction's personality trait value.
        /// </summary>
        /// <param name="factionID">Faction.</param>
        /// <param name="traitID">Trait ID.</param>
        /// <param name="value">New value.</param>
        public void SetPersonalityTrait(int factionID, int traitID, float value)
        {
            var faction = GetFaction(factionID);
            if (faction != null && (0 <= traitID && traitID < faction.traits.Length))
            {
                faction.traits[traitID] = value;
                personalityTraitChanged(factionID, traitID, value);
            }
        }

        /// <summary>
        /// Gets a relationship trait ID by its name.
        /// </summary>
        /// <returns>The relationship trait I.</returns>
        /// <param name="traitName">Trait name.</param>
        public int GetRelationshipTraitID(string traitName)
        {
            for (int i = 0; i < relationshipTraitDefinitions.Length; i++)
            {
                if (string.Equals(traitName, relationshipTraitDefinitions[i].name)) return i;
            }
            return -1;
        }

        /// <summary>
        /// Sets a faction's traits to the average of its parents' traits.
        /// </summary>
        /// <param name="factionName">Faction name.</param>
        public void InheritTraitsFromParents(string factionName)
        {
            InheritTraitsFromParents(GetFactionID(factionName), traitInheritanceType);
        }

        /// <summary>
        /// Sets a faction's traits to its inherited parents' traits.
        /// </summary>
        /// <param name="factionName">Faction name.</param>
        /// <param name="inheritanceType">How to handle inheritance.</param>
        public void InheritTraitsFromParents(string factionName, FactionInheritanceType inheritanceType)
        {
            InheritTraitsFromParents(GetFactionID(factionName));
        }

        /// <summary>
        /// Sets a faction's traits to the average of its parents' traits.
        /// </summary>
        /// <param name="factionID">Faction name.</param>
        public void InheritTraitsFromParents(int factionID)
        {
            InheritTraitsFromParents(factionID, traitInheritanceType);
        }

        /// <summary>
        /// Sets a faction's traits to its inherited parents' traits.
        /// </summary>
        /// <param name="factionID">Faction name.</param>
        /// <param name="inheritanceType">How to handle inheritance.</param>
        public void InheritTraitsFromParents(int factionID, FactionInheritanceType inheritanceType)
        {
            var faction = GetFaction(factionID);
            if (faction == null || faction.parents.Length < 1) return;
            faction.traits = GetInheritedTraits(factionID, inheritanceType, new List<int>());
        }

        private float[] GetInheritedTraits(int factionID, FactionInheritanceType inheritanceType, List<int> visited)
        {
            if (visited.Contains(factionID)) return null;
            visited.Add(factionID);
            var faction = GetFaction(factionID);
            if (faction == null) return null;
            if (faction.parents.Length < 1) return faction.traits;
            var total = Traits.Allocate(faction.traits.Length, true);
            int count = 0;
            for (int p = 0; p < faction.parents.Length; p++)
            {
                //--- No longer check ancestors; only check parents. 
                //--- Removed: var parentTraits = GetInheritedTraits(faction.parents[p], inheritanceType, visited);
                var parent = GetFaction(faction.parents[p]);
                if (parent != null && parent.traits != null)
                {
                    count++;
                    for (int t = 0; t < parent.traits.Length; t++)
                    {
                        total[t] += parent.traits[t];
                    }
                }
            }
            if (count == 0) return faction.traits;
            for (int t = 0; t < faction.traits.Length; t++)
            {
                total[t] = Mathf.Clamp(((inheritanceType == FactionInheritanceType.Sum) ? total[t] : total[t] / count), -100, 100);
            }

            return total;
        }

        /// <summary>
        /// Finds the personal relationship trait of a judging faction to a subject faction if it exists.
        /// </summary>
        /// <returns>`true`, if personal relationship was found, `false` otherwise.</returns>
        /// <param name="judgeFactionID">Judge faction ID.</param>
        /// <param name="subjectFactionID">Subject faction ID.</param>
        /// <param name="traitID">Trait ID.</param>
        /// <param name="value">Trait value of judge to subject.</param>
        public bool FindPersonalRelationship(int judgeFactionID, int subjectFactionID, out Relationship relationship)
        {
            var judge = GetFaction(judgeFactionID);
            if (judge == null)
            {
                relationship = null;
                return false;
            }
            return judge.FindPersonalRelationship(subjectFactionID, out relationship);
        }

        /// <summary>
        /// Finds the personal relationship trait of a judging faction to a subject faction if it exists.
        /// </summary>
        /// <returns>`true`, if personal relationship was found, `false` otherwise.</returns>
        /// <param name="judgeFactionID">Judge faction ID.</param>
        /// <param name="subjectFactionID">Subject faction ID.</param>
        /// <param name="traitID">Trait ID.</param>
        /// <param name="value">Trait value of judge to subject.</param>
        public bool FindPersonalRelationshipTrait(int judgeFactionID, int subjectFactionID, int traitID, out float value)
        {
            Relationship relationship;
            if (FindPersonalRelationship(judgeFactionID, subjectFactionID, out relationship))
            {
                value = relationship.GetTrait(traitID);
                return true;
            }
            else if ((judgeFactionID == subjectFactionID) && (traitID == Relationship.AffinityTraitIndex))
            {
                value = 100;
                return true;
            }
            value = 0;
            return false;
        }

        /// <summary>
        /// Finds the relationship trait of a judging faction to a subject faction.
        /// </summary>
        /// <returns>`true`, if relationship was found, `false` otherwise.</returns>
        /// <param name="judgeFactionID">Judge faction ID.</param>
        /// <param name="subjectFactionID">Subject faction ID.</param>
        /// <param name="traitID">Trait ID.</param>
        /// <param name="value">Trait value of judge to subject.</param>
        public bool FindRelationshipTrait(int judgeFactionID, int subjectFactionID, int traitID, out float value)
        {
            return FindRelationshipTraitRecursive(judgeFactionID, subjectFactionID, traitID, out value, 0, false);
        }

        private bool FindRelationshipTraitRecursive(int judgeFactionID, int subjectFactionID, int traitID, out float value, int depth, bool requireInheritable)
        {

            if (depth > MaxRecursionDepth)
            {
                Debug.LogWarning("Love/Hate: FindRelationshipTrait exceeded max parent search depth");
                value = 0;
                return false;
            }

            var judge = GetFaction(judgeFactionID);
            var subject = GetFaction(subjectFactionID);
            if (judge == null || subject == null)
            {
                value = 0;
                return false;
            }

            // Does judge have a relationship for target?
            Relationship relationship;
            if (judge.FindPersonalRelationship(subjectFactionID, out relationship))
            {
                if (relationship.inheritable || !requireInheritable)
                {
                    value = relationship.GetTrait(traitID);
                    return true;
                }
            }

            // Judging self and not handled by FindPersonalRelationship? (Return false.)
            if (judge == subject)
            {
                value = 0;
                return false;
            }

            // Does judge have a relationship for any of subject's parents?
            // (If so, return the average.)
            int numFound = 0;
            float total = 0;
            for (int s = 0; s < subject.parents.Length; s++)
            {
                var subjectParentFactionID = subject.parents[s];
                if (FindRelationshipTraitRecursive(judgeFactionID, subjectParentFactionID, traitID, out value, depth + 1, true))
                {
                    numFound++;
                    total += value;
                }
            }
            if (numFound > 0)
            {
                value = GetInheritedRelationshipValue(total, numFound);
                return true;
            }

            // Do any of judge's parents have an affinity for target or its parents?
            // (If so, return the average.)
            numFound = 0;
            total = 0;
            for (int j = 0; j < judge.parents.Length; j++)
            {
                var judgeParentFactionID = judge.parents[j];
                if (FindRelationshipTraitRecursive(judgeParentFactionID, subjectFactionID, traitID, out value, depth + 1, true))
                {
                    numFound++;
                    total += value;
                }
            }
            if (numFound > 0)
            {
                value = GetInheritedRelationshipValue(total, numFound);
                return true;
            }

            // Does judge's parents have an affinity for any of subject's parents?
            numFound = 0;
            total = 0;
            for (int j = 0; j < judge.parents.Length; j++)
            {
                var judgeParentFactionID = judge.parents[j];
                for (int s = 0; s < subject.parents.Length; s++)
                {
                    var subjectParentFactionID = subject.parents[s];
                    if (FindRelationshipTraitRecursive(judgeParentFactionID, subjectParentFactionID, traitID, out value, depth + 1, true))
                    {
                        numFound++;
                        total += value;
                    }
                }
            }
            if (numFound > 0)
            {
                value = GetInheritedRelationshipValue(total, numFound);
                return true;
            }

            value = 0;
            return false;
        }

        private float GetInheritedRelationshipValue(float total, float numFound)
        {
            return Mathf.Clamp((relationshipInheritanceType == FactionInheritanceType.Sum || Mathf.Approximately(numFound, 0)) ? total : (total / numFound), -100, 100);
        }

        /// <summary>
        /// Gets a relationship trait of a judging faction to a subject faction.
        /// </summary>
        /// <returns>The trait value.</returns>
        /// <param name="judgeFactionID">Judge faction ID.</param>
        /// <param name="subjectFactionID">Subject faction ID.</param>
        /// <param name="traitID">Trait ID.</param>
        public float GetRelationshipTrait(int judgeFactionID, int subjectFactionID, int traitID)
        {
            float value;
            return FindRelationshipTrait(judgeFactionID, subjectFactionID, traitID, out value)
                ? value : Relationship.GetDefaultValue(judgeFactionID, subjectFactionID, traitID);
        }

        /// <summary>
        /// Gets a judge's relationship trait to a subject using their faction names.
        /// </summary>
        /// <returns>The trait value.</returns>
        /// <param name="judgeFactionName">Judge faction name.</param>
        /// <param name="subjectFactionName">Subject faction name.</param>
        /// <param name="traitID">Trait ID.</param>
        public float GetRelationshipTrait(string judgeFactionName, string subjectFactionName, int traitID)
        {
            return GetRelationshipTrait(GetFactionID(judgeFactionName), GetFactionID(subjectFactionName), traitID);
        }

        /// <summary>
        /// Sets a faction's personal relationship trait to another faction.
        /// </summary>
        /// <param name="judgeFactionID">Judge faction ID.</param>
        /// <param name="subjectFactionID">Subject faction ID.</param>
        /// <param name="traitID">Trait ID.</param>
        /// <param name="value">Trait value.</param>
        public void SetPersonalRelationshipTrait(int judgeFactionID, int subjectFactionID, int traitID, float value)
        {
            var faction = GetFaction(judgeFactionID);
            if (faction == null) return;
            if (Mathf.Approximately(0, faction.percentJudgeParents))
            {
                faction.SetPersonalRelationshipTrait(subjectFactionID, traitID, value, relationshipTraitDefinitions.Length);
            }
            else
            {
                var previousValue = GetRelationshipTrait(judgeFactionID, subjectFactionID, traitID);
                var change = value - previousValue;
                var changeForParents = change * (faction.percentJudgeParents / 100);
                faction.SetPersonalRelationshipTrait(subjectFactionID, traitID, value, relationshipTraitDefinitions.Length);
                var subject = GetFaction(subjectFactionID);
                if (subject != null)
                {
                    for (int i = 0; i < subject.parents.Length; i++)
                    {
                        ModifyPersonalRelationshipTrait(judgeFactionID, subject.parents[i], traitID, changeForParents);
                    }
                }
            }
        }

        /// <summary>
        /// Sets a faction's personal relationship trait to another faction.
        /// </summary>
        /// <param name="judgeFactionName">Judge faction name.</param>
        /// <param name="subjectFactionName">Subject faction name.</param>
        /// <param name="traitID">Trait ID.</param>
        /// <param name="value">Trait value.</param>
        public void SetPersonalRelationshipTrait(string judgeFactionName, string subjectFactionName, int traitID, float value)
        {
            SetPersonalRelationshipTrait(GetFactionID(judgeFactionName), GetFactionID(subjectFactionName), traitID, value);
        }

        /// <summary>
        /// Modifies (increments or decrements) a judge's personal relationship trait to a subject 
        /// using their faction IDs.
        /// </summary>
        /// <param name="judgeFactionID">Judge faction ID.</param>
        /// <param name="subjectFactionID">Subject faction ID.</param>
        /// <param name="traitID">Trait ID.</param>
        /// <param name="change">Value change.</param>
        public void ModifyPersonalRelationshipTrait(int judgeFactionID, int subjectFactionID, int traitID, float change)
        {
            var currentValue = GetRelationshipTrait(judgeFactionID, subjectFactionID, traitID);
            SetPersonalRelationshipTrait(judgeFactionID, subjectFactionID, traitID, currentValue + change);
        }

        /// <summary>
        /// Modifies (increments or decrements) a judge's personal affinity to a subject 
        /// using their faction names.
        /// </summary>
        /// <param name="judgeFactionName">Judge faction name.</param>
        /// <param name="subjectFactionName">Subject faction name.</param>
        /// <param name="traitID">Trait ID.</param>
        /// <param name="change">Value change.</param>
        public void ModifyPersonalRelationshipTrait(string judgeFactionName, string subjectFactionName, int traitID, float change)
        {
            ModifyPersonalRelationshipTrait(GetFactionID(judgeFactionName), GetFactionID(subjectFactionName), traitID, change);
        }

        public void SetPersonalRelationshipInheritable(int judgeFactionID, int subjectFactionID, bool inheritable)
        {
            var faction = GetFaction(judgeFactionID);
            if (faction == null) return;
            faction.SetPersonalRelationshipInheritable(subjectFactionID, inheritable);
        }

        public void SetPersonalRelationshipInheritable(string judgeFactionName, string subjectFactionName, bool inheritable)
        {
            SetPersonalRelationshipInheritable(GetFactionID(judgeFactionName), GetFactionID(subjectFactionName), inheritable);
        }

        /// <summary>
        /// If the faction has personal relationship traits for a subject and the other doesn't, this
        /// gives them to the other based on the other's affinity for this faction.
        /// </summary>
        /// <param name="judgeFactionID">Judge faction ID (gives traits).</param>
        /// <param name="otherFactionID">Other faction ID (maybe receives traits).</param>
        /// <param name="subjectFactionID">Subject faction ID (traits about this faction).</param>
        public void ShareRelationshipTraits(int judgeFactionID, int otherFactionID, int subjectFactionID)
        {
            Relationship relationship;
            if (FindPersonalRelationship(judgeFactionID, subjectFactionID, out relationship))
            {
                float otherAffinityToSubject;
                if (!FindPersonalAffinity(otherFactionID, subjectFactionID, out otherAffinityToSubject))
                {
                    var otherAffinityToJudge = GetAffinity(otherFactionID, judgeFactionID);
                    var modifier = (otherAffinityToJudge / 100);
                    for (int traitID = 0; traitID < relationshipTraitDefinitions.Length; traitID++)
                    {
                        var value = relationship.GetTrait(traitID);
                        SetPersonalRelationshipTrait(otherFactionID, subjectFactionID, traitID, value * modifier);
                    }
                }
            }
        }

        /// <summary>
        /// Finds a faction's personal affinity for another faction. Doesn't check parents.
        /// </summary>
        /// <returns>`true` if personal affinity was found, `false` otherwise.</returns>
        /// <param name="subjectFactionID">The other faction ID.</param>
        /// <param name="affinity">Affinity of this faction to the subject.</param>
        public bool FindPersonalAffinity(int judgeFactionID, int subjectFactionID, out float affinity)
        {
            return FindPersonalRelationshipTrait(judgeFactionID, subjectFactionID, Relationship.AffinityTraitIndex, out affinity);
        }

        /// <summary>
        /// Finds the affinity of a judging faction to a subject faction.
        /// </summary>
        /// <returns>`true`, if affinity was found, `false` otherwise.</returns>
        /// <param name="judgeFactionID">Judge faction ID.</param>
        /// <param name="subjectFactionID">Subject faction ID.</param>
        /// <param name="affinity">Affinity of judge to subject.</param>
        public bool FindAffinity(int judgeFactionID, int subjectFactionID, out float affinity)
        {
            return FindRelationshipTrait(judgeFactionID, subjectFactionID, Relationship.AffinityTraitIndex, out affinity);
        }

        /// <summary>
        /// Gets the affinity of a judging faction to a subject faction.
        /// </summary>
        /// <returns>The affinity.</returns>
        /// <param name="judgeFactionID">Judge faction ID.</param>
        /// <param name="subjectFactionID">Subject faction ID.</param>
        public float GetAffinity(int judgeFactionID, int subjectFactionID)
        {
            return GetRelationshipTrait(judgeFactionID, subjectFactionID, Relationship.AffinityTraitIndex);
        }

        /// <summary>
        /// Gets a judge's affinity to a subject using their faction names.
        /// </summary>
        /// <returns>The affinity.</returns>
        /// <param name="judgeFactionName">Judge faction name.</param>
        /// <param name="subjectFactionName">Subject faction name.</param>
        public float GetAffinity(string judgeFactionName, string subjectFactionName)
        {
            return GetRelationshipTrait(judgeFactionName, subjectFactionName, Relationship.AffinityTraitIndex);
        }

        /// <summary>
        /// Sets this faction's personal affinity to another faction.
        /// </summary>
        /// <param name="subjectFactionID">Subject faction ID.</param>
        /// <param name="affinity">Affinity to the other faction.</param>
        public void SetPersonalAffinity(int judgeFactionID, int subjectFactionID, float affinity)
        {
            SetPersonalRelationshipTrait(judgeFactionID, subjectFactionID, Relationship.AffinityTraitIndex, affinity);
        }

        /// <summary>
        /// Sets a judge's personal affinity to a subject using their faction names.
        /// </summary>
        /// <param name="judgeFactionName">Judge faction name.</param>
        /// <param name="subjectFactionName">Subject faction name.</param>
        /// <param name="affinity">Affinity.</param>
        public void SetPersonalAffinity(string judgeFactionName, string subjectFactionName, float affinity)
        {
            SetPersonalRelationshipTrait(GetFactionID(judgeFactionName), GetFactionID(subjectFactionName), Relationship.AffinityTraitIndex, affinity);
        }

        /// <summary>
        /// Modifies (increments or decrements) a judge's personal affinity to a subject 
        /// using their faction IDs.
        /// </summary>
        /// <param name="judgeFactionID">Judge faction ID.</param>
        /// <param name="subjectFactionID">Subject faction ID.</param>
        /// <param name="affinityChange">Affinity change.</param>
        public void ModifyPersonalAffinity(int judgeFactionID, int subjectFactionID, float affinityChange)
        {
            ModifyPersonalRelationshipTrait(judgeFactionID, subjectFactionID, Relationship.AffinityTraitIndex, affinityChange);
        }

        /// <summary>
        /// Modifies (increments or decrements) a judge's personal affinity to a subject 
        /// using their faction names.
        /// </summary>
        /// <param name="judgeFactionName">Judge faction name.</param>
        /// <param name="subjectFactionName">Subject faction name.</param>
        /// <param name="affinityChange">Affinity change.</param>
        public void ModifyPersonalAffinity(string judgeFactionName, string subjectFactionName, float affinityChange)
        {
            ModifyPersonalRelationshipTrait(judgeFactionName, subjectFactionName, Relationship.AffinityTraitIndex, affinityChange);
        }

        /// <summary>
        /// If the faction has personal affinity for a subject and the other doesn't, this
        /// gives the other an affinity (and all other relationship traits) for the subject 
        /// based on the other's affinity for this faction.
        /// </summary>
        /// <param name="other">A member of a faction.</param>
        /// <param name="subjectFactionID">Subject faction ID.</param>
        public void ShareAffinity(int judgeFactionID, int otherFactionID, int subjectFactionID)
        {
            ShareRelationshipTraits(judgeFactionID, otherFactionID, subjectFactionID);
        }


    }

}
