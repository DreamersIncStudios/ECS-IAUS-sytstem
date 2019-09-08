using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IAUS
{
    [System.Serializable]
    public abstract class ConsiderationBase : IConsideration
    {
        public string NameId { get; set; }
        public bool Inverse { get; set; } // Is Invense Required if m is negatives?
        public float Score { get; set; }
        public CharacterContext Agent { get; set; }
        public abstract void Consider();
        public abstract void Output(float input);
        public ResponseType responseType { get; set; }
        public float M { get; set; }
        public float K { get; set; }
        public float B { get; set; }

        // public abstract IConsideration Clone();


    }
}
