using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Stats;

namespace AISenses
{
    public class UpdateSenses : ComponentSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((BaseCharacter character, ref LevelUpComponent level, ref Hearing hear)=>{ });
        }
    }
}