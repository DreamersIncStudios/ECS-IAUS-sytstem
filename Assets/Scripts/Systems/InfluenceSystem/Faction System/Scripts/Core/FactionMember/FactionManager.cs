using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DreamersInc.FactionSystem
{
    public class FactionManager : MonoBehaviour
    {
        public static FactionManager Instance;

        public static FactionDatabase Database;
        [SerializeField] FactionDatabase database;
        public void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
              Database = database;
            Database.IsPlaying = true;
        }
        public void OnDestroy()
        {
            Database.IsPlaying = false;
        }
    }
}