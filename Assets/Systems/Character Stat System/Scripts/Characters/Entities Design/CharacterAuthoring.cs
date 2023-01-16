using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using CharacterClass = Stats.Entities;
using DreamersInc;
namespace Stats.Entities
{
    public partial class CharacterAuthoring : MonoBehaviour
    {
        public CharacterClass Info;
        public Animator animator;
    }

    class ChararacterStatBaker : Baker<CharacterAuthoring>
    {
        public override void Bake(CharacterAuthoring authoring)
        {
            BaseCharacterComponent character = new();
            character.SetupDataEntity(authoring.Info);
                AddComponentObject (character);
            AddComponentObject(new AnimatorComponent() { anim = authoring.animator});
            AddComponent(new PlayerTag());
        }
    }

    public struct PlayerTag : IComponentData { } //Todo Determine is still needed with Player_Control?
    public struct NPCTag :IComponentData { }
}
