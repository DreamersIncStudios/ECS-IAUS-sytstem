using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VisualEffect
{
    public class DissolveSingle : MonoBehaviour
    {
        public Animator Anim;
        //TODO Event System needed

        public Material DissolveInstance;
        private void Awake()
        {
            DissolveInstance = this.GetComponent<Renderer>().material;

        }
        private void Start()
        {
            Anim = GetComponentInParent<Animator>();

        }
        float currentValue => DissolveInstance.GetFloat("Dissolve");
        float animValue => Anim.GetFloat("Dissolve");
        bool change => animValue != currentValue;
        // Update is called once per frame
        void Update()
        {
            if(change)
            DissolveInstance.SetFloat("Dissolve", animValue);
        }
    }
}