using Stats;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameModes.DestroyTheTower.TowerSystem
{
    [System.Serializable]
    public class TowerManager
    {
        public List<GameObject> TowerModels;
        public Transform Parent;
        public List<GameObject> TowersInScene { get; private set; }

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
                    TowersInScene.Add(GameObject.Instantiate(TowerModels[0], pos, Quaternion.identity));
                }
                else
                    goto Start;
            }
            foreach (GameObject go in TowersInScene) {
                go.GetComponent<TowerStats>().UpdateLevel(Level);
                go.transform.SetParent(Parent);
            }
        }




    }

}
