
using Unity.Entities;
using Unity.Mathematics;
namespace IAUS.ECS.Component
{
    public struct CharacterSpecs : IComponentData
    {
        public CharacterClass Class;

    }

    public enum CharacterClass {
        Citizen,
        Cop,
        Robber,

    }

}