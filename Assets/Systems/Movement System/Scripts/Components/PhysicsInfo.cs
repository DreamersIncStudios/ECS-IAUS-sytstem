using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics.Authoring;

namespace Global.Component {
    [System.Serializable]
    public struct PhysicsInfo : IComponentData
    {
        public PhysicsCategoryTags BelongsTo;
        public PhysicsCategoryTags CollidesWith;

    }
}