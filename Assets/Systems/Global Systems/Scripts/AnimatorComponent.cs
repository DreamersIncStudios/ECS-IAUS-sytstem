using UnityEngine;
using Unity.Entities;
namespace Stats.Entities
{
    public class AnimatorComponent : IComponentData
    {
        public Animator anim;
        public Rigidbody RB;
        public Transform transform;

    }
}
