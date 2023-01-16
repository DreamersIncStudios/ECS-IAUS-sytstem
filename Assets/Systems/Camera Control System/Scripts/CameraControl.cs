using System;
using UnityEngine;
using Cinemachine;

namespace DreamersStudio.CameraControlSystem
{
    public class CameraControl : MonoBehaviour
    {
        public CinemachineFreeLook Follow;
        public CinemachineFreeLook Target;
        public CinemachineTargetGroup TargetGroup;
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
                    TargetGroup.m_Targets[0].target = eventArgs.Target.transform;
            };

        }
        private void Update()
        {
            SetBias();
        }
        void SetBias() {
            if (playerCharacter == null)
            {
                playerCharacter = GameObject.FindGameObjectWithTag("Player");
                return;
            }
            Target.m_Heading.m_Bias = playerCharacter.transform.eulerAngles.y;
            
        }
    }
}
