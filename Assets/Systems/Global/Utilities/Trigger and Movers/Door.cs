using UnityEngine;
using static PrimeTween.Tween;

namespace LS
{
    public class Door : MonoBehaviour
    {
        [SerializeField,Header("Dependencies")]
        Transform doorPivot;

        [SerializeField,Range(0,2),Header("Settings")]
        float openDuration = 0.3f;
        
        [ContextMenu("Open Out")]
        public void OpenOut() => Rotation(doorPivot, Vector3.up * 90, openDuration);

        [ContextMenu("Open In")]
        public void OpenIn() => Rotation(doorPivot, Vector3.down * 90, openDuration);

        [ContextMenu("Close")]
        public void Close() => Rotation(doorPivot, Vector3.zero, openDuration);
    }
}