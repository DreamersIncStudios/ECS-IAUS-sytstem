using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DreamersInc.InflunceMapSystem {
    public sealed class FilterGroups
    {
        public  Dictionary<Faction, List<Faction>> Allies = new Dictionary<Faction, List<Faction>>();
        public  Dictionary<Faction, List<Faction>> Enemies = new Dictionary<Faction, List<Faction>>();

       public FilterGroups() {
            //TODO Read in Filter Data from JSON files
            Allies.Add(Faction.Enemy, new List<Faction>() { Faction.Faction4,Faction.Faction3 });
            Enemies.Add(Faction.Enemy, new List<Faction>() { Faction.Player });
            Allies.Add(Faction.NonCombative, new List<Faction> { Faction.Player });
            Enemies.Add(Faction.NonCombative, new List<Faction> { Faction.Enemy });

        }



    }
}