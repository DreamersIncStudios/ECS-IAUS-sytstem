using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
[GenerateAuthoringComponent]
public struct EnemyCharacter : IComponentData
{
    public LocalToWorld Target;
}
namespace InfluenceMap {

    public struct LookForPlayer : IComponentData { }
    public struct MoveToPlayer : IComponentData {
        public Gridpoint Location;
    }


}