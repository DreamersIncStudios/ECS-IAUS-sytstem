using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamersInc.Utils;
using System;
using Unity.Mathematics;

namespace DreamersInc.InflunceMapSystem
{
    public class InflunceGridObject
    {
        private const int MIN = 0;
        private const int MAX = 100;

        private GridGenericXZ<InflunceGridObject> grid;
        private Dictionary<Faction, int2> InfluenceValue;

        private int x, y;

        public InflunceGridObject(GridGenericXZ<InflunceGridObject> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
            InfluenceValue = new Dictionary<Faction, int2>();
        }


        public void AddValue(int2 addValue, Faction faction) {
            if (InfluenceValue.TryGetValue(faction, out int2 value))
            {
                InfluenceValue[faction]+= addValue;
                if (InfluenceValue[faction].Equals(int2.zero))
                    InfluenceValue.Remove(faction);

            }
            else
            {
                InfluenceValue.Add(faction, addValue);
            }

                    grid.TriggerGridObjectChanged(x, y);
            
    }
        /// <summary>
        /// Add value to grid in a diamond pattern
        /// </summary>
        /// <param name="value"></param>
        /// <param name="Totalrange"></param>
        public void AddValue( Vector3 worldPos, int2 value, int Totalrange, int fullValueRange, Faction faction) {
            grid.GetXZ(worldPos, out int originX, out int originZ);
            int2 lowerValueAmount = new int2() 
            {
                x = Mathf.RoundToInt((float)value.x / (Totalrange - fullValueRange)),
                y = Mathf.RoundToInt((float)value.y / (Totalrange - fullValueRange))
            };
            for (int x = 0; x < Totalrange; x++)
            {
                for (int z = 0; z < Totalrange-x; z++)
                {
                    int radius = x + y;
                    int2 addValueAmout = value;
                    if (radius > fullValueRange) {
                        addValueAmout -= lowerValueAmount * (radius - fullValueRange);
                    }
                    grid.GetGridObject(originX +x, originZ+z)?.AddValue(addValueAmout, faction);
                    if (x != 0)
                    {
                        grid.GetGridObject(originX - x, originZ + z)?.AddValue(addValueAmout, faction);
                    }
                    if (z != 0)
                    {
                        grid.GetGridObject(originX + x, originZ - z)?.AddValue(addValueAmout, faction);

                        if (x != 0)
                        {
                            grid.GetGridObject(originX - x, originZ - z)?.AddValue(addValueAmout, faction);
                        }
                    }
                }

            }
                
        }
        /// <summary>
        /// Returns influence value of a single faction
        /// </summary>
        /// <param name="faction"></param>
        /// <returns></returns>
        public int2 GetValue(Faction faction) {
            int2 value = new int2();
            InfluenceValue.TryGetValue(faction, out value);
            
            return value;
        }

        /// <summary>
        /// Return the combined influence of several factions 
        /// </summary>
        /// <param name="factions"></param>
        /// <returns></returns>
        public int2 GetValue(List<Faction> factions) { 
            int2 value = new int2();
            foreach (var item in factions)
            {
                if (InfluenceValue.TryGetValue(item, out int2 addValue)) {
                    value += addValue;
                }
            }
            return value;
        }

        public float2 GetValueNormalized(Faction faction) {
            return (float2)GetValue(faction)/ MAX;
        }

        public override string ToString()
        {
            return InfluenceValue.ToString();
        }
    }
    public enum Faction
    {
        Environmental, Player, Enemy, Faction2, Faction3, Faction4,//etc etc 
    }

}
