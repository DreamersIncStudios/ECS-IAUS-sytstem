using System;
using UnityEngine;
using Unity.Cinemachine;

namespace DreamersIncStudio.CameraControlSystem
{
    public class CameraControl : MonoBehaviour
    {
        public CinemachineCamera Follow;
        public CinemachineCamera Target;
        public static CameraControl Instance;
        public EventHandler<OnTargetingChangedEventArgs> OnTargetingChanged;
        GameObject playerCharacter;
        public class OnTargetingChangedEventArgs : EventArgs
        {
            public bool isTargeting;
        }
        public EventHandler<OnTargetChangedEventArgs> OnTargetChanged { get; set; }
        public class OnTargetChangedEventArgs : EventArgs
        {
            public GameObject Target;
            
            public OnTargetChangedEventArgs(GameObject target)
            {
                Target = target;
            }
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }
        private void Start()
        {
            OnTargetingChanged += (object sender, OnTargetingChangedEventArgs eventArgs) =>
            {
                if (eventArgs.isTargeting && Target.Priority != 15)
                {
                    Follow.Priority = 5;
                    Target.Priority = 15;
                }

                if (!eventArgs.isTargeting && Target.Priority == 15)
                {
                    Follow.Priority = 15;
                    Target.Priority = 5;
                }
            };
            OnTargetChanged += (object sender, OnTargetChangedEventArgs eventArgs) =>
            {
                Target.LookAt = eventArgs.Target != null ? eventArgs.Target.transform : null;
            };

        }

    }
}