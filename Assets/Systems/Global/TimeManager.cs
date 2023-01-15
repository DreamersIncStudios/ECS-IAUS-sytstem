using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DreamersIncStudios.Global { 
 

    public  class TimeManager: MonoBehaviour
    {
        public static TimeManager Manager;
        /// <summary>
        ///  How many real world minutes should a game hour take
        /// </summary>
        public uint TimeScale { get { return timeScale; } private set { timeScale = value; } }
        
        [Tooltip("How many real world minutes should a game hour take"), Range(1,60)]
        [SerializeField]  uint timeScale =2 ;


        public static bool SimulateTime { get; private set; }
         float curTime = 6;
        public int StartTime= 6;

        private int currentDay = 1;
        private  int currectMonth = 1;
        private int currentYear = 1;
        [SerializeField] bool fastForward;
        public float GetCurTime()
        {
            {
                return curTime;
            }
        }
        /// <summary>
        /// Set Time Scale on a range of 0 to 60 minutes per hour simulated
        /// </summary>
        /// <param name="scale"></param>
        public  void SetTimeScale(uint scale) {
            if (scale > 60)
                scale = 60;
            timeScale = scale;
        }
        /// <summary>
        /// Start or Stop the Flow of time
        /// </summary>
        /// <param name="simulate"></param>
        /// 
        public static void SimulateTimeActive(bool simulate) {
            SimulateTime = simulate;
        }

        private void Awake()
        {
            if (!Manager)
                Manager = this;
            else
                Destroy(this);
            SimulateTime = true;
            curTime = StartTime;
        }
       private bool Reset => curTime >= 23.99f;

        public Vector3 GetCurrentDate => new Vector3(currectMonth, currentDay, currentYear); 
        public void Update()
        {
            if (SimulateTime) {
                if (!fastForward)
                    curTime += Time.deltaTime / (60.0f * timeScale);
                else
                    curTime += Time.deltaTime * timeScale;

                if (Reset)
                {
                    curTime = 0.0f;
                    currentDay++;
                    if (currentDay > 30) {
                        currentDay = 1;
                        currectMonth++;
                        if (currectMonth > 12)
                        {
                            currectMonth = 1; 
                            currentYear++;
                        }
                    }
                }
            }
        }


    }
}