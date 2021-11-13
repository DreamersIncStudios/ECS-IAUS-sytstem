using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DreamersInc.FactionSystem
{
    [System.Serializable]
    public class GenusTribe :BasicInfo

    {
        public int[] factionChild;

        public List<Relationship> relationships;
        public GenusTribe() { }
        public GenusTribe(int id, string name)
        {
            this.id = id;
            this.name = name;
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