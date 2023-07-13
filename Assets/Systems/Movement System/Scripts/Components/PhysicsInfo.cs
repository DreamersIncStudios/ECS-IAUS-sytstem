using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics.Authoring;
using System;

namespace Global.Component {
    [Serializable]
    public struct PhysicsInfo : IComponentData
    {
        public PhysicsCategoryTags BelongsTo;
        public PhysicsCategoryTags CollidesWith;

    }
}