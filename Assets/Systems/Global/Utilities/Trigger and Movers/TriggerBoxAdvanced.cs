using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LS
{
    public class TriggerBoxAdvanced : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        BoxCollider boxCollider;
        [Header("Settings")]
        [SerializeField]
        TriggerBoxDebug debug;
        [SerializeField]
        public AdvancedTriggerEvent Changed;
        
        [SerializeField,Range(0,1),Header("Info")]
        float progess;

        void Awake() => actors = new HashSet<Collider>();

        void OnTriggerEnter(Collider other)
        {
            if(actors.Count == 0) first = other;
            actors.Add(other);
            
            CalculateTriggerProgress(other);
            OnEvent(IsCloserToA(other.transform.position) ? EventType.EnterA : EventType.EnterB);
        }

        void Update()
        {
            if(actors == null || actors.Count == 0) return;
            CalculateTriggerProgress(first);
            OnEvent(EventType.Move);
        }

        void OnTriggerExit(Collider other)
        {
            actors.Remove(other);
            if(actors.Count == 0) first = null;
            CalculateTriggerProgress(other);
            OnEvent(IsCloserToA(other.transform.position) ? EventType.ExitA : EventType.ExitB);
            progess = 0;
        }

        void OnDrawGizmos()
        {
            if(boxCollider == null)
                boxCollider = GetComponent<BoxCollider>();
         
            debug.DrawBox(boxCollider,transform,progess);
            if(actors == null || actors.Count == 0) return;
            debug.DrawProgress(transform,posA,posB,posAb,progess);
        }

        #region plumbing

        bool IsCloserToA(Vector3 pos) => Vector3.Distance(posA, pos) < Vector3.Distance(posB, pos);

        Vector3 GetIntersection(Vector3 a, Vector3 b, float distance, Vector3 target)
        {
            float t = ((target.x - b.x) * (a.x - b.x) +
                       (target.y - b.y) * (a.y - b.y) +
                       (target.z - b.z) * (a.z - b.z)) / (distance * distance);
            t = Mathf.Clamp(t, 0f, 1f);
            return new Vector3
            {
                x = b.x + t * (a.x - b.x),
                y = b.y + t * (a.y - b.y),
                z = b.z + t * (a.z - b.z)
            };
        }

        void CalculateTriggerProgress(Collider other) 
        {
            Vector3 pos = other.transform.position;
            Vector3 localPosition = transform.InverseTransformPoint(pos); 

            if(pos == previousPos) return;
            
            Vector3 a = new Vector3(-0.5f, 0, 0);
            Vector3 b = new Vector3(0.5f, 0, 0); 
            Vector3 ab = GetIntersection(a, b, 1, localPosition);

            var distanceToAB = Vector3.Distance(a, ab); 
            progess = Mathf.Clamp(distanceToAB, 0f, 1f);
            
            var size = boxCollider.size;
            var offset = Vector3.up * Mathf.Lerp(0, size.y/2, debug.TracklineHeight);
            
            posA = transform.TransformPoint(a+offset);
            posB = transform.TransformPoint(b+offset);
            posAb = transform.TransformPoint(ab + offset);
            
            previousPos = pos;
        }
        
        void OnEvent(EventType type) =>
            Changed.Invoke(new AdvancedTriggerArgs
            {
                Progress = progess,
                Collider = first,
                Event = type,
                APosition = posA,
                BPosition = posB
            });

        [Serializable]
        class TriggerBoxDebug
        {
            public bool Show = true;
            public Color BaseColor;
            public Color AColor;
            public Color BColor;
            [SerializeField,Range(0,0.5f)]
            public float ABSize = 0.15f;
            [SerializeField,Range(0,1)]
            public float TracklineHeight = 1f;

            public void DrawBox(BoxCollider box,Transform tr,float progress = 0f)
            {
                if(!Show) return;
                var size = box.size;
                var center = box.center;
                Gizmos.matrix = tr.localToWorldMatrix;
                Gizmos.color = BaseColor;
                Gizmos.DrawCube(center,size);
                Gizmos.color = AColor;
                Gizmos.DrawCube(new Vector3(-0.5f * size.x, 0f, 0f) + center, new Vector3(ABSize * size.x, 1f * size.y, 1f * size.z));
                Gizmos.color = BColor;
                Gizmos.DrawCube(new Vector3(0.5f * size.x, 0f, 0f) + center, new Vector3(ABSize * size.x, 1f * size.y, 1f * size.z));
                
                Gizmos.color = Color.green;
                if(progress > 0)
                    Gizmos.DrawWireCube(center,size);
            }

            public void DrawProgress(Transform t,Vector3 a,Vector3 b,Vector3 ab,float progress)
            {
                if (!Application.isPlaying) return;
 

                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = Color.white;
                Gizmos.DrawLine(a,b);
                Gizmos.color = Color.Lerp(AColor,BColor, progress);
                Gizmos.DrawSphere(ab, 0.1f);
            }
        }

        HashSet<Collider> actors;
        Vector3 previousPos;
        Vector3 posA,posB,posAb;
        Vector3 progress;
        Collider first;
        
        

        #endregion
        
    }

    #region Event Data

    [Serializable]
    public class AdvancedTriggerEvent : UnityEvent<AdvancedTriggerArgs>
    {
        
    }

    [Serializable]
    public class AdvancedTriggerArgs
    {
        public float Progress;
        public Collider Collider;
        public EventType Event;
        public Vector3 APosition;
        public Vector3 BPosition;
    }
    
    public enum EventType { EnterA, EnterB, ExitA, ExitB,Move}
    
    

    #endregion
    
}