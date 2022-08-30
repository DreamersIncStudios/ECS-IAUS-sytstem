using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DreamersInc.Utils;
using System;
using Unity.Mathematics;
using PixelCrushers.LoveHate;

namespace DreamersInc.InflunceMapSystem
{
    public class InfluenceGridObject
    {
        private const int MIN = 0;
        private const int MAX = 100;

        private GridGenericXZ<InfluenceGridObject> grid;
        private Dictionary<Faction, int2> gridValue;

        private int x, y;

        public InfluenceGridObject(GridGenericXZ<InfluenceGridObject> grid, int x, int y)
        {
            gridValue = new Dictionary<Faction, int2>();
            this.grid = grid;
            this.x = x;
            this.y = y;
        }


        public void AddValue(int2 addValue, Faction faction) {

            if (gridValue.ContainsKey(faction))
            {
                gridValue[faction] += addValue;
            }
            else
            {
                gridValue.Add(faction, addValue);
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
        //TODO add Zero out grid tiles 

        /// <summary>
        /// Add value to grid in a diamond pattern
        /// </summary>
        /// <param name="value"></param>
        /// <param name="Totalrange"></param>
        public void AddValue(int2 value, int Totalrange, int fullValueRange, Faction faction)
        {

            int2 lowerValueAmount = new int2()
            {
                x = Mathf.RoundToInt((float)value.x / (Totalrange - fullValueRange)),
                y = Mathf.RoundToInt((float)value.y / (Totalrange - fullValueRange))
            };
            for (int i = 0; i < Totalrange; i++)
            {
                for (int j = 0; j < Totalrange - i; j++)
                {
                    int radius = i + j;
                    int2 addValueAmout = value;
                    if (radius > fullValueRange)
                    {
                        addValueAmout -= lowerValueAmount * (radius - fullValueRange);
                    }
                    grid.GetGridObject(x + i, y + j)?.AddValue(addValueAmout, faction);
                    if (i != 0)
                    {
                        grid.GetGridObject(x - i, y + j)?.AddValue(addValueAmout, faction);
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

        public int2 GetValue(Faction faction) {

            List<Faction> foes = new List<Faction>();
            List<Faction> friends = new List<Faction>();
            List<Faction> neutral = new List<Faction>();

            foreach (var item in LoveHate.factionDatabase. GetFaction(faction.id).relationships)
            {
                if (item.GetTrait(0)>50)
                    friends.Add(LoveHate.factionDatabase.GetFaction(item.factionID));
                else if (item.GetTrait(0) < 50)
                    foes.Add(LoveHate.factionDatabase.GetFaction(item.factionID));
                else
                    neutral.Add(LoveHate.factionDatabase.GetFaction(item.factionID));
            }
            int2[] value = new int2[2];
            for (int i = 0; i < friends.Count; i++)
            {
                if (gridValue.TryGetValue(friends[i], out int2 output))
                {
                    value[0] += output;
                }
            }
            for (int i = 0; i < foes.Count; i++)
            {
                if (gridValue.TryGetValue(foes[i], out int2 output))
                {
                    value[1] += output;
                }
            }
            var output2 = new int2();
            output2.x = value[0].x;
            output2.y = value[1].y;
              //  new int2(value[0].x, value[1].y)
            return output2;
        }
        public float2 GetValueNormalized(Faction faction) {
            return (float2)GetValue(faction)/ MAX;
        }

        public float GetHighestThreatCell(Faction faction, bool filtered, out int i, out int j)
        {
            float HighValue = 0.0f;
            int startX, startY;
            startX = x - 50;
            startY = y - 50;
            i = j = 0;
            for (int SearchX = 0; SearchX < 100; SearchX++)
            {
                for (int SearchY = 0; SearchY < 100; SearchY++)
                {
                    float thisCellValue = grid.GetGridObject(startX + SearchX, startY + SearchY).GetValueNormalized(faction).y;
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
            startX = x - 25;
            startY = y - 25;
            i = j = 0;
            for (int SearchX = 0; SearchX < 50; SearchX++)
            {
                for (int SearchY = 0; SearchY < 50; SearchY++)
                {
                    float thisCellValue = grid.GetGridObject(startX + SearchX, startY + SearchY).GetValueNormalized(faction).y;
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
            string output ="";
            foreach (var val in gridValue)
            {
                output += val.ToString();
            }

            return output;
        }
    }


}
