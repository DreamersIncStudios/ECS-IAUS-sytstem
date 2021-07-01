using Unity.Entities;
namespace IAUS.ECS2.Component
{
    [GenerateAuthoringComponent]
    public struct AttackInfo : IComponentData
    {
        public float DistanceToTarget;
        public float AttackRange;
        public float Attacktimer; // This need to be derive from Character stats Possible ?
        public Entity Target;
        public bool InRangeForAttack => DistanceToTarget < AttackRange;

    }
}