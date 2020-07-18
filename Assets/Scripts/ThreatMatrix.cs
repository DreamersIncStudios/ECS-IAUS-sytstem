using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThreatMatrixSystem
{
    public class ThreatMatrix : MonoBehaviour
    {

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