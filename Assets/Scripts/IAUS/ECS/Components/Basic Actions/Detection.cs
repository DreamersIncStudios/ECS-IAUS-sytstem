using Unity.Entities;
using UnityEngine;

namespace IAUS.ECS.Component
{
    public struct Detection : IComponentData
{
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngleXZ;
    [Range(0, 360)]
    public float viewAngleYZ;

    public float EngageRadius;
    [Range(0, 360)]
    public float EngageViewAngle; //TBA
    public LayerMask TargetMask;
    public LayerMask ObstacleMask;
    public float distanceToClosetEnemy;
    public float distanceToClosetTarget;

    public float AlertModifer;
}
}