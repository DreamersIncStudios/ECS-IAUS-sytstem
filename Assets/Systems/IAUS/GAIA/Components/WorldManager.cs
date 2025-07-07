using Unity.Entities;
using UnityEngine;
using GameWorld = DreamersIncStudio.GAIACollective.WorldManager;
namespace DreamersIncStudio.GAIACollective.Authoring
{

    public class WorldManager : MonoBehaviour
    {
        public uint WorldLevel;

        public class baker : Baker<WorldManager>
        {
            public override void Bake(WorldManager authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new GameWorld(authoring));
            }
        }
    }
}

namespace DreamersIncStudio.GAIACollective
{
    public struct WorldManager : IComponentData
    {
        public uint WorldLevel;
        public uint PlayerLevel; // Move to player in game 
        public WorldManager(Authoring.WorldManager authoring)
        {
            WorldLevel = authoring.WorldLevel;
            PlayerLevel = 1;
        }
    }


}

