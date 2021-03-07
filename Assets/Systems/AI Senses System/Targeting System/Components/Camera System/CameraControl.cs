
using UnityEngine;
using Cinemachine;

namespace DreamersStudio.CameraControlSystem
{
    public class CameraControl : MonoBehaviour
    {
        public CinemachineFreeLook Follow;
        public CinemachineFreeLook Target;
        public bool isTargeting;
        public static CameraControl Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);
        }

        // Update is called once per frame
        void Update()
        {
            if (isTargeting && Target.Priority != 15)
            {
                Follow.Priority = 5;
                Target.Priority = 15;
            }

            if (!isTargeting && Target.Priority == 15)
            {
                Follow.Priority = 15;
                Target.Priority = 5;
            }
        }
        public void SwapFocus(Transform CharacterFocus)
        {
            Follow.Follow = CharacterFocus;
            Follow.LookAt = CharacterFocus.gameObject.GetComponentInChildren<FollowPointRef>().transform;

            Target.Follow = CharacterFocus;
            Target.LookAt = CharacterFocus.gameObject.GetComponentInChildren<FollowPointRef>().transform;
        }
    }
}