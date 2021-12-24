using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionSystem.Controls
{
    public class ControlMaster : MonoBehaviour
    {

        public ControllerScheme controller;
        public static ControlMaster Instance;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            if (Instance != this)
            {
                Destroy(this.gameObject);
            }
            DontDestroyOnLoad(this.gameObject);
        }

    }
}
