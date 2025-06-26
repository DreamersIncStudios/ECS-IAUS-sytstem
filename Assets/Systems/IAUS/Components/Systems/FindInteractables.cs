using System.Collections.Generic;
using IAUS.ECS.Component;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace IAUS.Components.Systems
{


    public partial struct FindInteractablesSystem : ISystem
    {

        EntityQuery query;
        private EntityQuery targets;



        void OnUpdate(ref SystemState state)
        {

        }


    }
}