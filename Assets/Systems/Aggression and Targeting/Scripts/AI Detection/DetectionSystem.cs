using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using IAUS.ECS2.BackGround.Raycasting;
using CharacterAlignmentSystem;
using IAUS.Core;

namespace IAUS.ECS2
{
    [UpdateInGroup(typeof(IAUS_UpdateConsideration))] // make a separate group
    [DisableAutoCreation]
    public partial class DetectionSystemJob : JobComponentSystem
    {
        public NativeArray<Entity> AttackableEntityInScene;
        public EntityQueryDesc AttackableQuery = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(CharacterAlignment), typeof(LocalToWorld) }
        };


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            ComponentDataFromEntity<LocalToWorld> Positions = GetComponentDataFromEntity<LocalToWorld>(true);
            NativeArray<Entity> attackableEntities = GetEntityQuery(AttackableQuery).ToEntityArray(Allocator.TempJob);
            JobHandle Gettargets = Entities
                .WithNativeDisableParallelForRestriction(Positions)
                .WithReadOnly(Positions)
                .WithDeallocateOnJobCompletion(attackableEntities)
                .WithReadOnly(attackableEntities)

                .ForEach((Entity entity, ref DynamicBuffer<TargetBuffer> buffer, ref Detection c1) =>
                {
                    buffer.Clear();

                    for (int index = 0; index < attackableEntities.Length; index++)
                    {
                        float dist = Vector3.Distance(Positions[attackableEntities[index]].Position, Positions[entity].Position);
                        if (dist <= c1.viewRadius)
                        {
                            Vector3 dirToTarget = ((Vector3)Positions[attackableEntities[index]].Position - (Vector3)Positions[entity].Position).normalized;
                            if (Vector3.Angle(Positions[entity].Forward, dirToTarget) < c1.viewAngleXZ / 2.0f)
                            {
                                buffer.Add(new TargetBuffer()
                                {
                                    target = attackableEntities[index],
                                });
                            }
                        }
                    }
                })
                .Schedule(inputDeps);
            return Gettargets;

        }

    }
    [UpdateInGroup(typeof(IAUS_UpdateConsideration))]
    [UpdateAfter(typeof(DetectionSystemJob))]

    public partial class DetectionSystem : ComponentSystem
    {
        public NativeArray<Entity> AttackableEntityInScene;
        public EntityQueryDesc AttackableQuery = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(CharacterAlignment), typeof(LocalToWorld) }
        };

        ComponentDataFromEntity<CharacterAlignment> AttackableComponents;
        ComponentDataFromEntity<HumanRayCastPoints> RaycastPoints;


        protected override void OnUpdate()
        {
            AttackableComponents = GetComponentDataFromEntity<CharacterAlignment>();
            RaycastPoints = GetComponentDataFromEntity<HumanRayCastPoints>();

            Entities.ForEach(( DynamicBuffer<TargetBuffer> Targets,ref LocalToWorld localToWorld, ref Detection c1) =>
            {
                int check = new int();
                List<float> EnemyDetectionLevels = new List<float>();

                    while (check < Targets.Length)
                    {
                        CharacterAlignment temp = AttackableComponents[Targets[check].target];
                        JobHandle jobHandle;
                        NativeList<RaycastCommand> CastRayEnemy = new NativeList<RaycastCommand>(Allocator.Persistent);

                        switch (temp.Type)
                        {
                            case ObjectType.Creature:
                                break;
                            case ObjectType.Humaniod:
                                var HumanRaySetup = new SetupHumanRayCast()
                                {
                                    AgentPos = localToWorld,
                                    detect = c1,
                                    humanRaysTargets = RaycastPoints[Targets[check].target],
                                    RaysToSetup = CastRayEnemy
                                };
                                jobHandle = HumanRaySetup.Schedule();
                                jobHandle.Complete();
                                CastRayEnemy = HumanRaySetup.RaysToSetup;
                                break;
                            case ObjectType.Structure:
                                break;

                        }

                        NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(CastRayEnemy.Length, Allocator.Persistent);
                        RaycastCommand.ScheduleBatch(CastRayEnemy, results, 1).Complete();
                        float alert = new float();
                        switch (temp.Type)
                        {
                            case ObjectType.Creature:
                                break;
                            case ObjectType.Humaniod:
                                alert = CheckHumanRays(results);
                                break;
                            case ObjectType.Structure:
                                break;

                        }
                        results.Dispose();
                        CastRayEnemy.Dispose();
                        EnemyDetectionLevels.Add(alert);

                        check++;
                    }
                

                if (EnemyDetectionLevels.Count > 0)
                {
                    c1.TargetVisibility = Mathf.Max(EnemyDetectionLevels.ToArray());
                    int indexofMaxDetection = EnemyDetectionLevels.IndexOf(c1.TargetVisibility);
                    c1.TargetRef = Targets[indexofMaxDetection].target;
                }
                else
                {
                    c1.TargetVisibility = 0.0f;
                    c1.TargetRef = Entity.Null;
                }

            });
        }

    }
    
}
