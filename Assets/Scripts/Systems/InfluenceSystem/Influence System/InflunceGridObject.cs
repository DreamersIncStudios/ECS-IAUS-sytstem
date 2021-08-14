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
        private int2 Enemy;
        private int2 Player;

        private int x, y;

        public InflunceGridObject(GridGenericXZ<InflunceGridObject> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
        }


        public void AddValue(int2 addValue, Faction faction) {
            switch (faction)
            {

                case Faction.Enemy:
                    Enemy += addValue;
                    Enemy.x = Mathf.Clamp(Enemy.x, MIN, MAX);
                    Enemy.y = Mathf.Clamp(Enemy.y, MIN, MAX);
                    break;

                case Faction.Player:
                    Player += addValue;
                    Player.x = Mathf.Clamp(Player.x, MIN, MAX);
                    Player.y = Mathf.Clamp(Player.y, MIN, MAX);
                    break;
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
        public int2 GetValue(Faction faction) {
            int2 value = new int2();
            switch(faction){
                case Faction.Player:
                    value = Player;
                    break;
                case Faction.Enemy:
                    value = Enemy;
                    break;
            }

            return value;
        }
        public float2 GetValueNormalized(Faction faction) {
            return (float2)GetValue(faction)/ MAX;
        }

        public override string ToString()
        {
            return Player.ToString();
        }
    }
    public enum Faction
    {
        None, Player, Enemy, Faction2, Faction3, Faction4,//etc etc 
    }

}
