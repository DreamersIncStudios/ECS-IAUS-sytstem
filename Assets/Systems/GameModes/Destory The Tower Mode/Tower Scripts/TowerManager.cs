using Stats;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestiaryLibrary;
using Global.Component;

namespace GameModes.DestroyTheTower.TowerSystem
{
    [System.Serializable]
    public class TowerManager
    {
        public List<GameObject> TowerModels;
        public Transform Parent;
        public List<GameObject> TowersInScene { get; private set; }
        public PhysicsInfo physicsInfo;
        public TowerManager() {
        }
       public void LoadGOFromResources()
        {
            //Todo List all model to load;
           GameObject temp = Resources.Load("TowerBasic") as GameObject;
            TowerModels.Add(temp);
        }
        public void SpawnTower(int Level, int count) {

            TowersInScene = new List<GameObject>();
            // TODO update later
            for (int i = 0; i < count; i++)
            {
                Start:
                if (Utilities.GlobalFunctions.RandomPoint(Vector3.zero, 100, out Vector3 pos))
                {
                    BestiaryDB.SpawnTowerAndCreateEntityData(pos, physicsInfo, out GameObject goRef);
                    TowersInScene.Add(goRef);
                }
                else
                    goto Start;
            }
          
        }




    }

}
