using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IAUS
{
    public abstract class ActionBase : IAction
    {
        public string NameId { get; }

        public List<ConsiderationBase> Considerations { get;set; }

        public float TotalScore { get; }

        public abstract void Execute();
        public abstract void Setup();
        public abstract void Score();
 
    }
}