using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DreamersInc.FactionSystem
{
    [System.Serializable]
    public class BasicInfo {
        public int id;
        public string name = string.Empty; // should this be Get Private Set?
        public string description = string.Empty;
        [Range(-100, 100)] public int EnemyThreshold, FriendlyThreshold;
        public float[] traits = new float[0];

    }

    [System.Serializable]
    public class Faction:BasicInfo // Should this be sealed?
    {
        public int[] genusTribes = new int[0];

        public List<Relationship> relationships;
        public Faction() { }
        public Faction(int id, string name) {
            this.id = id;
            this.name = name;
        }

        public bool HasDirectGenusTribe(int genusTribeID)
        {
            for (int p = 0; p < genusTribes.Length; p++)
            {
                if (genusTribes[p] == genusTribeID) return true;
            }
            return false;
        }
        public void AddDirectGenusTribe(int genusTribeID)
        {
            if (HasDirectGenusTribe(genusTribeID)) return;
            var newGenusTribes = new List<int>(genusTribes);
            newGenusTribes.Add(genusTribeID);
            genusTribes = newGenusTribes.ToArray();
        }
        public void RemoveDirectGenusTribe(int genusTribeID)
        {
            if (!HasDirectGenusTribe(genusTribeID)) return;
            var newGenusTribes = new List<int>(genusTribes);
            newGenusTribes.Remove(genusTribeID);
            genusTribes = newGenusTribes.ToArray();
        }
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
        public bool HasPersonalRelationship(int factionID)
        {
            Relationship relationship;
            return FindPersonalRelationship(factionID, out relationship);
        }

        public void RemovePersonalRelationship(int factionID)
        {
            relationships.RemoveAll(r => r.factionID == factionID);
        }
        public float GetPersonalRelationshipTrait(int factionID, int traitID)
        {
            Relationship relationship;
            return FindPersonalRelationship(factionID, out relationship)
                ? relationship.GetTrait(traitID)
                    : Relationship.GetDefaultValue(id, factionID, traitID);
        }

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


        public void SetPersonalRelationshipInheritable(int factionID, bool inheritable)
        {
            Relationship relationship;
            if (FindPersonalRelationship(factionID, out relationship))
            {
                relationship.inheritable = inheritable;
            }
        }



    }
}