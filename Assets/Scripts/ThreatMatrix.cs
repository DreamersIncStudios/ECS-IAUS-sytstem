using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThreatMatrixSystem
{
    public class ThreatMatrix : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }


    public enum Crew {
        Knights,
        Bandits,
        Demons,
        Slayers,
    }
    public struct Threat {
        public Crew Indentifier;
        public int Value;

    }

}