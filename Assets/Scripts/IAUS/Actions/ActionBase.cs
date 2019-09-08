using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IAUS
{[System.Serializable]
    public abstract class ActionBase : IAction
    {
        public string NameId { get; set; }

        public List<ConsiderationBase> Considerations { get;set; }
        public CharacterContext Agent { get; set; }
  
        public float TotalScore { get; set; }

        public abstract void Execute();
        public abstract void Setup();
        public virtual void Score()
        {
            TotalScore = 1.0f;

            foreach (ConsiderationBase consideration in Considerations)
            {
                consideration.Consider();
                TotalScore= TotalScore *consideration.Score;
               // Debug.Log(consideration.Score+ " "+ consideration.NameId);
            }
        }
            
 
    }
}