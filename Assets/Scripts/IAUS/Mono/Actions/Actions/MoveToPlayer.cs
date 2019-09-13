using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.Considerations;

namespace IAUS.Actions
{
    public class MoveToPlayer : ActionBase
    {
       
        public override void Setup()
        {
            Considerations = new List<ConsiderationBase>();
            Considerations.Add(new CharacterHealth { Agent = Agent });
            Considerations.Add(new PlayerInSight { Agent = Agent });
        }


        public override void OnStart()
        {
            throw new System.NotImplementedException();
        }

        public override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }

        public override void OnExit()
        {
            throw new System.NotImplementedException();
        }


     
    }
}