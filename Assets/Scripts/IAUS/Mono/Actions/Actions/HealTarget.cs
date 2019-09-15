using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IAUS.Considerations;
namespace IAUS.Actions
{
    public class HealTarget : ActionBase
    {
        public Target Target;

        public override void Setup()
        {
            base.Setup();
            Considerations.Add(new CharacterHealth
            {
                Agent = Agent,
                NameId = "Health",
                responseType = ResponseType.Logistic,
                M = 5,
                K = -.85f,
                B = 1f,
                C = .4f
            });
            Considerations.Add(new CharacterMana
            {
                Agent = Agent,
                NameId = "Mana",
                responseType = ResponseType.Logistic,
                M = 5,
                K = .85f,
                B = 0.155f,
                C = .4f
            });
        }
        public override void OnExit()
        {
            throw new System.NotImplementedException();
        }

        public override void OnStart()
        {
            throw new System.NotImplementedException();
        }

        public override void OnUpdate()
        {
            throw new System.NotImplementedException();
        }

 



    }


    public enum Target {
        Player,
        Self,
        Allies
    }
}