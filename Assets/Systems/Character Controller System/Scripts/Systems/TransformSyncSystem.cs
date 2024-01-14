using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace MotionSystem
{
    public partial class TransformSyncSystem : SystemBase
    {
        protected override void OnUpdate()
        {
           Entities.WithoutBurst().ForEach((TransformGO go, ref LocalTransform local) => {
               local.Position = go.transform.position;
               local.Rotation = go.transform.rotation;
           }).Run();
        }
    }
}
