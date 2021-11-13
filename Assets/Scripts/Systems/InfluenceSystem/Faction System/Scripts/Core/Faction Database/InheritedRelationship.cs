using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace DreamersInc.FactionSystem
{
    public class InheritedRelationship : System.IComparable
    {
        public string name;
        public float affinity;

        public InheritedRelationship(string name, float affinity)
        {
            this.name = name;
            this.affinity = affinity;
        }

        public int CompareTo(object obj)
        {
            var other = obj as InheritedRelationship;
            if (string.IsNullOrEmpty(name) && other == null) return 0;
            if (string.IsNullOrEmpty(name)) return -1;
            if (other == null) return 1;
            return name.CompareTo(other.name);
        }

        public static List<InheritedRelationship> GetInheritedRelationships(FactionDatabase database, int factionID)
        {
            return GetInheritedRelationships(database, factionID, new List<int>());
        }
        private static List<InheritedRelationship> GetInheritedRelationships(FactionDatabase database, int factionID, List<int> visited)
        {
            var list = new List<InheritedRelationship>();
            var subjectIDs = new List<int>();
            AddInheritedRelationships(database, factionID, new List<int>(), list, subjectIDs);
            //AddAveragedRelationships(database, factionID, list, subjectIDs);
            list.Sort();
            return list;
        }

        private static void AddInheritedRelationships(FactionDatabase database, int tribeID, List<int> visited, List<InheritedRelationship> list, List<int> subjectIDs)
        {
            if (database == null || visited.Contains(tribeID)) return;
            visited.Add(tribeID);
            var faction = database.GetFaction(tribeID);
            if (faction != null)
            {
                for (int p = 0; p < faction.genusTribes.Length; p++)
                {
                    var parentID = faction.genusTribes[p];
                    var parent = database.GetGenusTribe(parentID);
                    if (parent != null)
                    {
                        for (int r = 0; r < parent.relationships.Count; r++)
                        {
                            var relationship = parent.relationships[r];
                            if (relationship.inheritable)
                            {
                                var subject = database.GetGenusTribe(relationship.factionID);
                                if (subject != null)
                                {
                                    list.Add(new InheritedRelationship(subject.name + " (" + parent.name + ")", relationship.affinity));
                                    if (!subjectIDs.Contains(subject.id))
                                    {
                                        subjectIDs.Add(subject.id);
                                    }
                                }
                            }
                        }
                        AddInheritedRelationships(database, parentID, visited, list, subjectIDs);
                    }
                }
            }
        }

        private static void AddAveragedRelationships(FactionDatabase database, int factionID, List<InheritedRelationship> list, List<int> subjectIDs)
        {
            if (database == null || list == null || subjectIDs == null) return;
            for (int i = 0; i < subjectIDs.Count; i++)
            {
                var subject = database.GetFaction(subjectIDs[i]);
                if (subject != null)
                {
                    var affinity = database.GetAffinity(factionID, subject.id);
                    list.Add(new InheritedRelationship(subject.name + " [inherited]", affinity));
                }
            }
        }
    }
}