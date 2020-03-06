using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Unity.Burst;
using IAUS.ECS2.BackGround.Raycasting;
namespace IAUS.ECS2
{

    [UpdateAfter(typeof(StateScoreSystem))]
    public class DetectionSystem : ComponentSystem
    {
        public NativeArray<Entity> AttackableEntityInScene;
        public EntityQueryDesc AttackableQuery = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(Attackable), typeof(LocalToWorld) }
        };

        ComponentDataFromEntity<Attackable> AttackableComponents;
        ComponentDataFromEntity<HumanRayCastPoints> HumanRayCastPoints;
       // ComponentDataFromEntity<Attackable> AttackableComponents;
       // ComponentDataFromEntity<Attackable> AttackableComponents;

        protected override  void OnUpdate()
        {
            AttackableComponents = GetComponentDataFromEntity<Attackable>();
            HumanRayCastPoints = GetComponentDataFromEntity<HumanRayCastPoints>();
            JobHandle handle = new GetListOfTarget()
            {
                attackableEntities = GetEntityQuery(AttackableQuery).ToEntityArray(Allocator.TempJob),
                Positions = GetComponentDataFromEntity<LocalToWorld>(true)

            }.Schedule(this);
            handle.Complete();
             
            Entities.ForEach((Entity entity, ref Detection c1, ref LocalToWorld localToWorld)=>
            {
                BufferFromEntity<TargetBuffer> targetBuffer = GetBufferFromEntity<TargetBuffer>(true);
                int check = new int();

                if (targetBuffer.Exists(entity))
                {
                    DynamicBuffer<TargetBuffer> Targets = targetBuffer[entity];
                    while (check < Targets.Length)
                    {
                        Attackable temp = AttackableComponents[Targets[check].target];
                        JobHandle jobHandle;
                        NativeList<RaycastCommand> CastRayEnemy = new NativeList<RaycastCommand>(Allocator.Persistent);

                        switch (temp.Type) {
                            case ObjectType.Animal:
                                break;
                            case ObjectType.Humaniod:
                                var HumanRaySetup = new SetupHumanRayCast()
                                {
                                    AgentPos = localToWorld,
                                    detect = c1,
                                    humanRaysTargets = HumanRayCastPoints[Targets[check].target],
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

                        results.Dispose();
                       CastRayEnemy.Dispose();



                        check++;
                    }
                }
              // CastRayEnemy.Dispose();

            });


        }

        public bool Hit(RaycastHit Result, Detection DetectSpecs)
        {
            Collider col = Result.collider;

            if (Result.collider != null)
            {
                return col.gameObject.layer == 8;
            }
            return false;
        }
    }

    [BurstCompile]
    struct GetListOfTarget : IJobForEachWithEntity_EBC<TargetBuffer,Detection>
    {
        [DeallocateOnJobCompletion] [ReadOnly]public NativeArray<Entity> attackableEntities;
        [NativeDisableParallelForRestriction][ReadOnly] public ComponentDataFromEntity<LocalToWorld> Positions;

        public void Execute(Entity entity, int Tindex, DynamicBuffer<TargetBuffer> Target, ref Detection c1)
        {
            Target.Clear();

            for (int index = 0; index < attackableEntities.Length; index++) {
                float dist = Vector3.Distance(Positions[attackableEntities[index]].Position, Positions[entity].Position);
                if (dist <= c1.viewRadius)
                {
                    Vector3 dirToTarget = ((Vector3)Positions[attackableEntities[index]].Position - (Vector3)Positions[entity].Position).normalized;
                    if (Vector3.Angle(Positions[entity].Forward, dirToTarget) < c1.viewAngleXZ / 2.0f) {
                        Target.Add(new TargetBuffer()
                        {
                            target = attackableEntities[index],
                        } );

                    }
                }

            }
        }
    }
    [BurstCompile]
    public struct SetupHumanRayCast : IJob
    {
        public LocalToWorld AgentPos;
        public Detection detect;
        public HumanRayCastPoints humanRaysTargets;
        public NativeList<RaycastCommand> RaysToSetup;
        public void Execute()
        {
            //Chest Ray
            float dist = Vector3.Distance(humanRaysTargets.Chest, AgentPos.Position);
            Vector3 DirTo = ((Vector3)humanRaysTargets.Chest - (Vector3)AgentPos.Position).normalized;
            RaycastCommand temp = new RaycastCommand()
            {
                from = AgentPos.Position,
                direction = DirTo,
                distance = dist,
                layerMask = ~detect.ObstacleMask,
                maxHits = 1

            };
            RaysToSetup.Add(temp);
            //Head Ray
            dist = Vector3.Distance(humanRaysTargets.Head, AgentPos.Position);
            DirTo = ((Vector3)humanRaysTargets.Head - (Vector3)AgentPos.Position).normalized;
            temp = new RaycastCommand()
            {
                from = AgentPos.Position,
                direction = DirTo,
                distance = dist,
                layerMask = ~detect.ObstacleMask,
                maxHits = 1

            };
            RaysToSetup.Add(temp);
            //Right arm Ray
            dist = Vector3.Distance(humanRaysTargets.Right_Arm, AgentPos.Position);
            DirTo = ((Vector3)humanRaysTargets.Right_Arm - (Vector3)AgentPos.Position).normalized;
            temp = new RaycastCommand()
            {
                from = AgentPos.Position,
                direction = DirTo,
                distance = dist,
                layerMask = ~detect.ObstacleMask,
                maxHits = 1

            };
            RaysToSetup.Add(temp);
            //Left arm Ray
            dist = Vector3.Distance(humanRaysTargets.Left_Arm, AgentPos.Position);
            DirTo = ((Vector3)humanRaysTargets.Left_Arm - (Vector3)AgentPos.Position).normalized;
            temp = new RaycastCommand()
            {
                from = AgentPos.Position,
                direction = DirTo,
                distance = dist,
                layerMask = ~detect.ObstacleMask,
                maxHits = 1

            };
            RaysToSetup.Add(temp);
            //Righ LegRay
            dist = Vector3.Distance(humanRaysTargets.Right_Leg, AgentPos.Position);
            DirTo = ((Vector3)humanRaysTargets.Right_Leg - (Vector3)AgentPos.Position).normalized;
            temp = new RaycastCommand()
            {
                from = AgentPos.Position,
                direction = DirTo,
                distance = dist,
                layerMask = ~detect.ObstacleMask,
                maxHits = 1

            };
            RaysToSetup.Add(temp);
            //Left leg Ray
            dist = Vector3.Distance(humanRaysTargets.Left_Leg, AgentPos.Position);
            DirTo = ((Vector3)humanRaysTargets.Left_Leg - (Vector3)AgentPos.Position).normalized;
            temp = new RaycastCommand()
            {
                from = AgentPos.Position,
                direction = DirTo,
                distance = dist,
                layerMask = ~detect.ObstacleMask,
                maxHits = 1

            };
            RaysToSetup.Add(temp);
        }
    }
}