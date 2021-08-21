using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamersInc.Utils;
using System;
using Unity.Mathematics;

namespace DreamersInc.InflunceMapSystem
{
    public class InfluenceGridObject
    {
        private const int MIN = 0;
        private const int MAX = 100;

        private GridGenericXZ<InfluenceGridObject> grid;
        private Dictionary<Faction, int2> InfluenceValue;

        private int x, y;

        public InfluenceGridObject(GridGenericXZ<InfluenceGridObject> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
            InfluenceValue = new Dictionary<Faction, int2>();
        }


        public void AddValue(int2 addValue, Faction faction) {
            if (InfluenceValue.TryGetValue(faction, out int2 value))
            {
                value.x = Mathf.Clamp(value.x + addValue.x, MIN, MAX);
                value.y = Mathf.Clamp(value.y + addValue.y, MIN, MAX);
                InfluenceValue[faction] = value;
              
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
        public void AddValue( int2 value, int Totalrange, int fullValueRange, Faction faction) {
          
            int2 lowerValueAmount = new int2() 
            {
                x = Mathf.RoundToInt((float)value.x / (Totalrange - fullValueRange)),
                y = Mathf.RoundToInt((float)value.y / (Totalrange - fullValueRange))
            };
            for (int i = 0; i < Totalrange; i++)
            {
                for (int j = 0; j < Totalrange-i; j++)
                {
                    int radius = i + j;
                    int2 addValueAmout = value;
                    if (radius > fullValueRange) {
                        addValueAmout -= lowerValueAmount * (radius - fullValueRange);
                    }
                    grid.GetGridObject(x +i, y+j)?.AddValue(addValueAmout, faction);
                    if (i != 0)
                    {
                        grid.GetGridObject(x- i, y + j)?.AddValue(addValueAmout, faction);
                    }
                    if (j != 0)
                    {
                        grid.GetGridObject(x + i, y - j)?.AddValue(addValueAmout, faction);

                        if (i != 0)
                        {
                            grid.GetGridObject(x - i, y - j)?.AddValue(addValueAmout, faction);
                        }
                    }
                }

            }
                
        }

        /// <summary>
        /// Return the combined influence of several factions 
        /// </summary>
        /// <param name="factions"></param>
        /// <returns></returns>
        public int2 GetValue(Faction faction, bool Filtered) {
            int2 value = new int2();

            if (Filtered)
            {
                int2 enemyValue = new int2();
                int2 proxValue = new int2();

                List<Faction> enemyFilter = InfluenceGridMaster.Filters.Enemies[faction];
                List<Faction> proxFilter = InfluenceGridMaster.Filters.Allies[faction];

                foreach (var item in enemyFilter)
                {
                    if (InfluenceValue.TryGetValue(item, out int2 addValue))
                    {
                        enemyValue += addValue;
                    }
                }

                foreach (var item in proxFilter)
                {
                    if (InfluenceValue.TryGetValue(item, out int2 addValue))
                    {
                        proxValue += addValue;
                    }
                }
                value.x = proxValue.x;
                value.y = enemyValue.y;
            }
            else
            {
                InfluenceValue.TryGetValue(faction, out value);
            }


            return value;
        }

        public float2 GetValueNormalized(Faction faction, bool filtered)
        {
            if (filtered)
            {

                int2 factions = new int2()
                {
                    x = InfluenceGridMaster.Filters.Allies[faction].Count,
                    y = InfluenceGridMaster.Filters.Enemies[faction].Count
                };
                return (float2)GetValue(faction, true) / (MAX * factions);
            }
            else
            {
                return (float2)GetValue(faction, false) / MAX;
            }
        }


        public float GetHighestThreatCell(Faction faction, bool filtered, out int i, out int j) {
            float HighValue = 0.0f;
            int startX, startY;
            startX = x - 50;
            startY = y - 50;
            i = j= 0;
            for (int SearchX = 0; SearchX < 100; SearchX++)
            {
                for (int SearchY = 0; SearchY < 100; SearchY++)
                {
                    float thisCellValue = grid.GetGridObject(startX+SearchX,startY +SearchY).GetValueNormalized(faction, filtered).y;
                    if (thisCellValue > HighValue)
                    {
                        HighValue = thisCellValue;
                        i = startX + SearchX;
                        j = startY + SearchY;
                    }
                }
            }
            return HighValue;
        }
      
        public float GetLowestThreatCell(Faction faction, bool filtered, out int i, out int j)
        {
            float LowValue = 1.0f;
            int startX, startY;
            startX = x - 50;
            startY = y - 50;
            i = j = 0;
            for (int SearchX = 0; SearchX < 100; SearchX++)
            {
                for (int SearchY = 0; SearchY < 100; SearchY++)
                {
                    float thisCellValue = grid.GetGridObject(startX + SearchX, startY + SearchY).GetValueNormalized(faction, filtered).y;
                    if (thisCellValue < LowValue)
                    {
                        LowValue = thisCellValue;
                        i = startX + SearchX;
                        j = startY + SearchY;
                    }
                }
            }
            return LowValue;
        }
      
        public override string ToString()
        {
            string output = int2.zero.ToString();

            if (InfluenceValue.TryGetValue(Faction.Player, out int2 value)) {
                output = value.ToString();
            }

            return output;
        }
    }
    public enum Faction
    {
        Environmental, Player, Enemy, Faction2, Faction3, Faction4,//etc etc 
    }

}
