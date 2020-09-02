using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace CharacterAlignmentSystem
{
    public class CheckSystem : SystemBase
    {
        private EntityQuery testingFactions;
        protected override void OnCreate()
        {
            base.OnCreate();
            testingFactions = GetEntityQuery(new EntityQueryDesc() { 
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(FactionBase)) }
            });
        }

        protected override void OnUpdate()
        {
         
        }
    }
}
