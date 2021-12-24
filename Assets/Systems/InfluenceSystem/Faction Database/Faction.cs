// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;
using System;
using System.Collections.Generic;

namespace PixelCrushers.LoveHate
{

    /// <summary>
    /// Defines a faction record for the faction database. The scope of this class
    /// is an individual faction, with no awareness of other factions except as 
    /// faction IDs in the parents and relationships arrays.
    /// </summary>
    [Serializable]
    public class Faction
    {

        /// <summary>
        /// The faction's unique ID.
        /// </summary>
        public int id;

        /// <summary>
        /// The name of the faction.
        /// </summary>
        public string name = string.Empty;

        /// <summary>
        /// The description of the faction.
        /// </summary>
        public string description = string.Empty;

        /// <summary>
        /// The color of the faction's gizmo in the scene view.
        /// </summary>
        public int color = 0;

        /// <summary>
        /// The parent factions. You only need to specify direct parents, as those parent
        /// factions may have their own parents (this faction's grandparents) and so on.
        /// </summary>
        public int[] parents = new int[0];

        /// <summary>
        /// The faction's personality traits.
        /// </summary>
        public float[] traits = new float[0];

        /// <summary>
        /// The relationships this faction feels toward other factions.
        /// </summary>
        public List<Relationship> relationships = new List<Relationship>();

        /// <summary>
        /// If nonzero, when setting a relationship to a subject also modify relationships to the 
        /// subject's parents scaled by this amount.
        /// </summary>
        public float percentJudgeParents = 0;

        public Faction() { }

        public Faction(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        /// <summary>
        /// Determines whether this faction has a direct parent with the specified parentID.
        /// </summary>
        /// <returns><c>true</c> if this faction has direct parent with the specified parentID; otherwise, <c>false</c>.</returns>
        /// <param name="parentID">Parent ID.</param>
        public bool HasDirectParent(int parentID)
        {
            for (int p = 0; p < parents.Length; p++)
            {
                if (parents[p] == parentID) return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a direct parent to this faction.
        /// </summary>
        /// <param name="parentID">Parent ID.</param>
        public void AddDirectParent(int parentID)
        {
            if (HasDirectParent(parentID)) return;
            var newParents = new List<int>(parents);
            newParents.Add(parentID);
            parents = newParents.ToArray();
        }

        /// <summary>
        /// Removes a direct parent from this faction.
        /// </summary>
        /// <param name="parentID">Parent ID.</param>
        public void RemoveDirectParent(int parentID)
        {
            if (!HasDirectParent(parentID)) return;
            var newParents = new List<int>(parents);
            newParents.Remove(parentID);
            parents = newParents.ToArray();
        }

        /// <summary>
        /// Searches for a personal relationship from this faction to another.
        /// </summary>
        /// <returns><c>true</c>, if a personal relationship was found, <c>false</c> otherwise.</returns>
        /// <param name="factionID">Faction ID to search for.</param>
        /// <param name="relationship">Relationship (if found).</param>
        public bool FindPersonalRelationship(int factionID, out Relationship relationship)
        {
            for (int r = 0; r < relationships.Count; r++)
            {
                var current = relationships[r];
                if (current.factionID == factionID)
                {
                    relationship = current;
                    return true;
                }
            }
            relationship = null;
            return false;
        }

        /// <summary>
        /// Determines whether this faction has a personal relationship the specified factionID.
        /// </summary>
        /// <returns><c>true</c> if this faction has a personal relationship the specified factionID; otherwise, <c>false</c>.</returns>
        /// <param name="factionID">Faction ID.</param>
        public bool HasPersonalRelationship(int factionID)
        {
            Relationship relationship;
            return FindPersonalRelationship(factionID, out relationship);
        }

        /// <summary>
        /// Removes the personal relationship record that this faction has for another faction, if
        /// that relationship record exists.
        /// </summary>
        /// <param name="factionID">Faction ID.</param>
        public void RemovePersonalRelationship(int factionID)
        {
            relationships.RemoveAll(r => r.factionID == factionID);
        }

        /// <summary>
        /// Gets a personal relationship trait to another faction.
        /// </summary>
        /// <returns>The personal relationship trait, or `0` if no personal relationship.
        /// If no personal relationship and the request is for affinity to self, returns `100`.</returns>
        /// <param name="factionID">Faction ID.</param>
        /// <param name="traitID">Trait ID.</param>
        public float GetPersonalRelationshipTrait(int factionID, int traitID)
        {
            Relationship relationship;
            return FindPersonalRelationship(factionID, out relationship)
                ? relationship.GetTrait(traitID)
                    : Relationship.GetDefaultValue(id, factionID, traitID);
        }

        /// <summary>
        /// Sets a personal relationship trait to another faction.
        /// </summary>
        /// <param name="factionID">Faction ID.</param>
        /// <param name="traitID">Trait ID.</param>
        /// <param name="value">Value.</param>
        /// <param name="numTraits">Size of the traits list. Needed if this method has to create a new relationship traits list.</param>
        public void SetPersonalRelationshipTrait(int factionID, int traitID, float value, int numTraits)
        {
            Relationship relationship;
            if (FindPersonalRelationship(factionID, out relationship))
            {
                relationship.SetTrait(traitID, value);
            }
            else
            {
                var newRelationship = Relationship.GetNew(factionID, new float[numTraits]);
                newRelationship.SetTrait(traitID, value);
                relationships.Add(newRelationship);
            }
        }

        /// <summary>
        /// Sets a personal relationship inheritable or not inheritable.
        /// </summary>
        /// <param name="factionID">Faction ID</param>
        /// <param name="value">Inheritable value</param>
        public void SetPersonalRelationshipInheritable(int factionID, bool inheritable)
        {
            Relationship relationship;
            if (FindPersonalRelationship(factionID, out relationship))
            {
                relationship.inheritable = inheritable;
            }
        }

        /// <summary>
        /// Gets the personal affinity to another faction.
        /// </summary>
        /// <returns>The personal affinity, or `0` if no personal relationship.</returns>
        /// <param name="factionID">Faction ID.</param>
        public float GetPersonalAffinity(int factionID)
        {
            return GetPersonalRelationshipTrait(factionID, Relationship.AffinityTraitIndex);
        }

        /// <summary>
        /// Sets the personal affinity to another faction.
        /// </summary>
        /// <param name="factionID">Faction ID.</param>
        /// <param name="affinity">Affinity.</param>
        /// <param name="numTraits">Size of the traits list. Needed if this method has to create a new relationship traits list.</param>
        public void SetPersonalAffinity(int factionID, float affinity, int numTraits)
        {
            SetPersonalRelationshipTrait(factionID, Relationship.AffinityTraitIndex, affinity, numTraits);
        }

        public static string[] ColorNames = new string[]
        {
            "White",
            "Black",
            "Blue",
            "Brown",
            "Cyan",
            "Green",
            "Lime",
            "Magenta",
            "Orange",
            "Pink",
            "Red",
            "Yellow"
        };

        public static string[] GizmoIconNames = new string[]
        {
            "LoveHate/FactionMember_White",
            "LoveHate/FactionMember_Black",
            "LoveHate/FactionMember_Blue",
            "LoveHate/FactionMember_Brown",
            "LoveHate/FactionMember_Cyan",
            "LoveHate/FactionMember_Green",
            "LoveHate/FactionMember_Lime",
            "LoveHate/FactionMember_Magenta",
            "LoveHate/FactionMember_Orange",
            "LoveHate/FactionMember_Pink",
            "LoveHate/FactionMember_Red",
            "LoveHate/FactionMember_Yellow"
        };

    }

}
