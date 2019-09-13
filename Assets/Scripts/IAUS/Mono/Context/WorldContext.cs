using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAUS {
    public class WorldContext : MonoBehaviour, IContext
    {

        public static WorldContext World;
        public GameObject Player { get { return GameObject.FindGameObjectWithTag("Player"); } }

        public void Awake()
        {
            if (World == null)
                World = this;
            if (World != this)
                Destroy(this.gameObject);
        }
        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}