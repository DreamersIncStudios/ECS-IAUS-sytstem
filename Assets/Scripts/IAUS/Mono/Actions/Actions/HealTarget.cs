//Copyright 2019 <Dreamers Inc Studios>
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



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
            base.OnExit();
        }

        public override void OnStart()
        {
            base.OnExit();
        }

        public override void Execute()
        {
            base.Execute();
            throw new System.NotImplementedException();
        }

 



    }


    public enum Target {
        Player,
        Self,
        Allies
    }
}