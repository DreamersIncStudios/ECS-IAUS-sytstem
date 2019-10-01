//Copyright 2019 <Dreamers Inc Studios>
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



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
        public ActionStatus actionStatus { get;set; } = ActionStatus.Idle;

        bool CanExecute()
        {
            if (InCooldown)
            {
                actionStatus = ActionStatus.Failure;
                return false;
            }

            return true;
        }
        public bool InCooldown
        {
            get
            {
                if (actionStatus == ActionStatus.Running ||
                   actionStatus == ActionStatus.Idle)
                    return false;

                return CooldownTimer <= 0.0f;
            }
        }

        public float Cooldown { get; set; }

        public float CooldownTimer { get; set; }



        public virtual void Execute() {

            if (actionStatus != ActionStatus.Running) { OnStart(); }
        }
        public virtual void OnExit() {
            actionStatus = ActionStatus.Idle;
            CooldownTimer -=Time.deltaTime;
        }
        public virtual void OnStart() {
            actionStatus = ActionStatus.Running;
            CooldownTimer = Cooldown;
        }
        public virtual void Setup() {
            Considerations = new List<ConsiderationBase>();

        }
        public virtual void Score()
        {
            TotalScore = 1.0f;

            foreach (ConsiderationBase consideration in Considerations)
            {
                consideration.Consider();

                TotalScore= TotalScore *consideration.Score;
                float mod = 1.0f - (1.0f / (float)Considerations.Count);
                float MakeUp = (1.0F - TotalScore) * mod;
                TotalScore =  Mathf.Clamp01(TotalScore + (MakeUp * TotalScore));
            }
        }

  

    }
}