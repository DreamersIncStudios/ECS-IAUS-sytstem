//Copyright 2019 <Dreamers Inc Studios>
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using IAUS.Considerations;
using Unity.Jobs;
using Unity.Collections;


namespace IAUS.Actions
{
    [System.Serializable]
    public class PatrolArea : ActionBase
    {

        public Transform Target { get { return Agent.Target; } set { Agent.Target = value; } }
        float TimeAtLoc { get { return Agent.TimeAtLoc; } }
        public override void Setup()
        {
            base.Setup();
   

            //Considerations = new List<ConsiderationBase>();
            Considerations.Add(new DistanceTO {
                NameId = "Distance To Next Waypoint",
                Agent = Agent,
                Point = Target,
                responseType = ResponseType.Logistic,
                M = 50,
                K = .85f,
                B = 0.15f,
                C = .6f
               
            });
          // Considerations.Add(new PlayerInSight { NameId = "Can I see Player" });
           Considerations.Add(new WaitConsideration { NameId = "WaitAtPoint", Agent = Agent    ,                         
               responseType = ResponseType.Logistic,
               M = 5,
               K = -.99f,
               B = 1.0f,
               C = 0.6f
           });
            Considerations.Add(new CharacterHealth
            {
                Agent = Agent,
                NameId = "Health",
                responseType = ResponseType.Logistic,
                M = 5,
                K = .85f,
                B = 0.155f,
                C = .4f
            });
        }

        public override void OnStart()
        {
            base.OnStart();


                Agent.Moving = true;
            
        }
        public override void Execute()
        {
            base.Execute();
            if (actionStatus != ActionStatus.Running) { OnStart(); }
            Agent.NavAgent.SetDestination(Target.position);

            float dist = Vector3.Distance(Agent.transform.position, Target.position);
            

            if ( dist < 1.5f) { OnExit(); }

        }
        public override void OnExit()
        {
            base.OnExit();
            Agent.Moving = false;
            Agent.Timer = Agent.TimeAtLoc;
            Agent.ExitTimerLoop = true;
        }
    }

}