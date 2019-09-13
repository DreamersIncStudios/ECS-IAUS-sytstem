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

        public abstract void OnUpdate();
        public abstract void OnExit();
        public abstract void OnStart();
        public abstract void Setup();
        public virtual void Score()
        {
            TotalScore = 1.0f;

            foreach (ConsiderationBase consideration in Considerations)
            {
                consideration.Consider();

                TotalScore= TotalScore *consideration.Score;
                float mod = 1.0f - (1.0f / (float)Considerations.Count);
                float MakeUp = (1.0F - TotalScore) * mod;
                TotalScore = TotalScore + (MakeUp * TotalScore);
            }
        }
            
 
    }
}