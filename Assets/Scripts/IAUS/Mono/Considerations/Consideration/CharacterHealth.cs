using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IAUS.Considerations
{
    public class CharacterHealth : ConsiderationBase
    {
        int CurHealth { get { return Agent.PC.CurHealth; } }
        int CurMana { get { return Agent.PC.MaxHealth; } }
        int MaxHealth { get { return Agent.PC.CurMana; } }
        int MaxMana { get { return Agent.PC.MaxMana; } }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void Consider()
        {
            float input = (float)CurMana / (float)MaxMana * ((float)CurHealth / (float)MaxHealth);
        }
    }
}