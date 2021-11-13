using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DreamersInc.FactionSystem
{

    public delegate void PersonalityTraitChangedDelegate(int factionID, int traitID, float value);

    public class FactionDatabase : ScriptableObject
    {
        /// <summary>
        /// Faction ID 0 is reserved for the player.
        /// </summary>
        public const int PlayerFactionID = 0;
        public const int PlayerTribeID = 0;
        public Faction[] factions = new Faction[0];
        public GenusTribe[] genusTribes = new GenusTribe[0];

        private Dictionary<int, Faction> m_factionIDLookup = new Dictionary<int, Faction>();

        private Dictionary<string, Faction> m_factionNameLookup = new Dictionary<string, Faction>();
        public Preset[] presets = new Preset[0];
        public List<Relationship> relationships;

        private Dictionary<int, GenusTribe> genusTribeIDLookup = new Dictionary<int, GenusTribe>();

        private Dictionary<string, GenusTribe> genusTribeNameLookup = new Dictionary<string, GenusTribe>();
        public TraitDefinition[] personalityTraitDefinitions = new TraitDefinition[0];
        public TraitDefinition[] relationshipTraitDefinitions = new TraitDefinition[1] { new TraitDefinition("Affinity", "(Required)") };

        /// <summary>
        /// The next faction ID, used by the custom editor to auto-number factions.
        /// </summary>
        public int nextFactionID = 0;
        public int nextGenusTribeID = 0;
        #region Faction

        public int CreateNewFaction(string factionName, string description)
        {
            var newFaction = new Faction(nextFactionID++, factionName);
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
        private void SetFactionLookups(Faction faction)
        {
            SetFactionIDLookup(faction.id, faction);
            SetFactionNameLookup(faction.name, faction);
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
        public bool IsPlaying;
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
        
        public FactionInheritanceType traitInheritanceType = FactionInheritanceType.Average;

        public Faction LookupFactionByIDRuntime(int factionID) {
            if (m_factionIDLookup.ContainsKey(factionID))
                return m_factionIDLookup[factionID];
            else
                throw new ArgumentOutOfRangeException(nameof(factionID), $"Faction ID {factionID} does not exist in database." );
        }




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
        #endregion

        #region GenusTribe
        public int CreateNewGenusTribe(string genusTribeName, string description)
        {
            var newGenusTribe = new GenusTribe(nextFactionID++, genusTribeName);
            newGenusTribe.description = description;
            newGenusTribe.traits = new float[personalityTraitDefinitions.Length];
             for (int i = 0; i < newGenusTribe.traits.Length; i++)
             {
                newGenusTribe.traits[i] = 0;
             }

            List<GenusTribe> genusList = new List<GenusTribe>(genusTribes);
            genusList.Add(newGenusTribe);
            genusTribes = genusList.ToArray();
            // SetFactionLookups(newFaction);
            return newGenusTribe.id;
        }

        public GenusTribe GetGenusTribe(int genusTribeID)
        {
            return LookupGenusTribeByID(genusTribeID);
        }

        /// <summary>
        /// Gets the faction with the specified name.
        /// </summary>
        /// <returns>The faction, or `null` if no faction exists by that name.</returns>
        /// <param name="factionName">Faction name.</param>
        public GenusTribe GetGenusTribe(string factionName)
        {
            return LookupGenusTribeByName(factionName);
        }

        /// <summary>
        /// Gets a faction's ID from its name.
        /// </summary>
        /// <returns>The faction ID, or `-1` if no faction exists by that name.</returns>
        /// <param name="factionName">Faction name.</param>
        public int GetGenusTribeID(string factionName)
        {
            return LookupFactionIDByName(factionName);
        }

        private GenusTribe LookupGenusTribeByName(string genusTribeName)
        {
            if (Application.isPlaying && genusTribeNameLookup.TryGetValue(genusTribeName, out GenusTribe tribeOut))
            {
                return tribeOut;
            }
            else
            {
                for (int f = 0; f < genusTribes.Length; f++)
                {
                    var tribe = genusTribes[f];
                    if ((tribe != null) && string.Equals(tribe.name, genusTribeName))
                    {
                        if (Application.isPlaying) genusTribeNameLookup.Add(genusTribeName, tribe);
                        return tribe;
                    }
                }
                return null;
            }
        }

        private int LookupGenusTribeIDByName(string factionName)
        {
            var faction = LookupFactionByName(factionName);
            return (faction != null) ? faction.id : -1;
        }

        private GenusTribe LookupGenusTribeByID(int genusTribeID)
        {
            if (Application.isPlaying && genusTribeIDLookup.TryGetValue(genusTribeID, out GenusTribe tribe))
            {
                return tribe;
            }
            else
            {
                for (int f = 0; f < genusTribes.Length; f++)
                {
                    var genus= genusTribes[f];
                    if ((genus != null) && (genus.id == genusTribeID))
                    {
                        if (Application.isPlaying) genusTribeIDLookup.Add(genusTribeID, genus);
                        return genus;
                    }
                }
                return null;
            }
        }
        #endregion

        #region Personality Traits
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

        public event PersonalityTraitChangedDelegate personalityTraitChanged = delegate { };

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


        #endregion
        public void Initialize()
        {
            personalityTraitDefinitions = new TraitDefinition[0];
            relationshipTraitDefinitions = new TraitDefinition[1] { new TraitDefinition("Affinity", "(Required)") };
         //   presets = new Preset[0];
            factions = new Faction[1] { new Faction(PlayerFactionID, "Player") };
            genusTribes = new GenusTribe[1] { new GenusTribe(PlayerTribeID, "Player Tribe") };
            nextFactionID = PlayerFactionID + 1;
        }
        public float GetRelationshipTrait(int judgeFactionID, int subjectFactionID, int traitID)
        {
            float value;
            return FindRelationshipTrait(judgeFactionID, subjectFactionID, traitID, out value)
                ? value : Relationship.GetDefaultValue(judgeFactionID, subjectFactionID, traitID);
        }

        public float GetRelationshipTrait(string judgeFactionName, string subjectFactionName, int traitID)
        {
            return GetRelationshipTrait(GetFactionID(judgeFactionName), GetFactionID(subjectFactionName), traitID);
        }

        public float GetAffinity(int judgeFactionID, int subjectFactionID)
        {
            return GetRelationshipTrait(judgeFactionID, subjectFactionID, Relationship.AffinityTraitIndex);
        }
        public float GetAffinity(string judgeFactionName, string subjectFactionName)
        {
            return GetRelationshipTrait(judgeFactionName, subjectFactionName, Relationship.AffinityTraitIndex);
        }
        public bool FindRelationshipTrait(int judgeFactionID, int subjectFactionID, int traitID, out float value)
        {
            return FindRelationshipTraitRecursive(judgeFactionID, subjectFactionID, traitID, out value, 0, false);
        }

        /// <summary>
        /// The max recursion depth, used to prevent infinite loops.
        /// </summary>
        private const int MaxRecursionDepth = 128;

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
            for (int s = 0; s < subject.genusTribes.Length; s++)
            {
                var subjectParentFactionID = subject.genusTribes[s];
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
            for (int j = 0; j < judge.genusTribes.Length; j++)
            {
                var judgeParentFactionID = judge.genusTribes[j];
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
            for (int j = 0; j < judge.genusTribes.Length; j++)
            {
                var judgeParentFactionID = judge.genusTribes[j];
                for (int s = 0; s < subject.genusTribes.Length; s++)
                {
                    var subjectParentFactionID = subject.genusTribes[s];
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

        public FactionInheritanceType relationshipInheritanceType = FactionInheritanceType.Average;

        private float GetInheritedRelationshipValue(float total, float numFound)
        {
            return Mathf.Clamp((relationshipInheritanceType == FactionInheritanceType.Sum || Mathf.Approximately(numFound, 0)) ? total : (total / numFound), -100, 100);
        }
    }

    public enum FactionInheritanceType
    {
        /// <summary>
        /// Average the parents' values.
        /// </summary>
        Average,

        /// <summary>
        /// Sum the parents' values.
        /// </summary>
        Sum
    }
}