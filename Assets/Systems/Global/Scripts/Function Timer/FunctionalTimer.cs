using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Systems.Global.Function_Timer
{
    public sealed class FunctionTimer
    {
        private static List<FunctionTimer> activeTimers;
        private static GameObject initGameObject;
        private static void InitIfNeeded()
        {
            if (initGameObject == null)
            {
                initGameObject = new GameObject("FuntionTimer_Init");
                activeTimers = new List<FunctionTimer>();
            }
        }

        public static FunctionTimer Create(Action Act, float timer, string timerName = null, bool repeat = false)
        {
            InitIfNeeded();
            GameObject go = new GameObject("FunctionTimer", typeof(MonoBehaviourHook));

            FunctionTimer functionTimer = new FunctionTimer(Act, timer, go, timerName);
            go.GetComponent<MonoBehaviourHook>().onUpdate = functionTimer.Update;
            activeTimers.Add(functionTimer);
            return functionTimer;
        }

        public static void StopTimer(string timerName, bool StopRepeat = true)
        {
            for (int i = 0; i < activeTimers.Count; i++)
            {
                if (activeTimers[i].timerName == timerName)
                {
                    activeTimers[i].repeat = false;
                    activeTimers[i].DestorySelf();
                    break;
                }
            }
        }


        public static void RemoveTimer(FunctionTimer functionTimer)
        {
            InitIfNeeded();
            activeTimers.Remove(functionTimer);

        }
        public class MonoBehaviourHook : MonoBehaviour
        {
            public Action onUpdate;
            public void Update()
            {
                if (onUpdate != null)
                    onUpdate();
            }
        }

        private Action action;
        public float timer { get; private set; }
        private bool isDestroyed = false;
        private GameObject functionGameObject;
        private string timerName;
        private bool repeat = false;
        private float startingTime;
        private FunctionTimer(Action act, float timer, GameObject functionGameObject, string timerName)
        {
            this.action = act;
            this.timer = timer;
            this.startingTime = timer;
            this.isDestroyed = false;
            this.timerName = timerName;
            this.functionGameObject = functionGameObject;
        }
        public void Update()
        {
            if (!isDestroyed)
            {
                timer -= Time.deltaTime;
                if (timer < 0)
                {
                    action();
                    DestorySelf();
                }
            }

        }
        private void DestorySelf()
        {
            isDestroyed = true;
            UnityEngine.Object.Destroy(functionGameObject);
            if (repeat)
            {
                Create(action, startingTime, timerName, repeat);
            }
            RemoveTimer(this);
        }
    }
}
