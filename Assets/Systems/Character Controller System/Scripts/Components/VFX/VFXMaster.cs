using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace DreamersInc.CharacterControllerSys.VFX
{
    public class VFXMaster : MonoBehaviour
    {
        class baker : Baker<VFXMaster>
        {
            public override void Bake(VFXMaster authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponentObject(entity, new VFXSpawnMaster());
                AddComponent(entity, new vfxTag());

            }
        }
    }
    
    public class VFXSpawnMaster : IComponentData {
        public VFXDatabase VFXspawn;
    }
    public struct vfxTag : IComponentData { };


}