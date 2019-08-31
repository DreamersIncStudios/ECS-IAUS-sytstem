using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IAUS
{
    [System.Serializable]
    public abstract class ConsiderationBase : IConsideration
    {
        public string NameId { get; set; }
        public bool Inverse { get; set; }
        public float Score { get; set; }

        public abstract void Consider();


       // public abstract IConsideration Clone();


    }
}
