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
                Any = new ComponentType[] { ComponentType.ReadOnly(typeof(Angel)),ComponentType.ReadOnly(typeof(Human)),ComponentType.ReadOnly(typeof(Daemon)),
                    ComponentType.ReadOnly(typeof(Mecha)) }
            });
        }

        protected override void OnUpdate()
        {
         
        }
    }
}
