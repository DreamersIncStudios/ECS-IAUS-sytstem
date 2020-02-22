using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
//Add namespace later

[GenerateAuthoringComponent]
public struct EnemyCharacter : IComponentData
{
    public Entity Target;
    public bool HaveTarget;
    public InfluenceMap.Gridpoint gridpoint;
}
namespace InfluenceMap {

    public struct LookForPlayer : IComponentData {
        public int test;
    }
    public struct MoveToPlayer : IComponentData {
        public Gridpoint Location;
        public bool CanMoveToPlayer;
    }


}