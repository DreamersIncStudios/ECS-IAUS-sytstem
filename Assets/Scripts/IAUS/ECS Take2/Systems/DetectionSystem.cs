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
    public partial class DetectionSystem : ComponentSystem
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
                List<float> EnemyDetectionLevels = new List<float>();

                if (targetBuffer.Exists(entity))
                {
                    DynamicBuffer<TargetBuffer> Targets = targetBuffer[entity];
                    while (check < Targets.Length)
                    {
                        Attackable temp = AttackableComponents[Targets[check].target];
                        JobHandle jobHandle;
                        NativeList<RaycastCommand> CastRayEnemy = new NativeList<RaycastCommand>(Allocator.Persistent);

                        switch (temp.Type) {
                            case ObjectType.Creature:
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
                        float alert = new float();
                        switch (temp.Type)
                        {
                            case ObjectType.Creature:
                                break;
                            case ObjectType.Humaniod:
                                alert = CheckHumanRays(results, c1);
                                break;
                            case ObjectType.Structure:
                                break;

                        }
                        results.Dispose();
                       CastRayEnemy.Dispose();
                        EnemyDetectionLevels.Add(alert);

                        check++;
                    }
                }
                
                int indexofMaxDetection = EnemyDetectionLevels.IndexOf(Mathf.Max(EnemyDetectionLevels.ToArray()));
                Debug.Log(indexofMaxDetection);
              // CastRayEnemy.Dispose();

            });
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
                        //add logic for sort by type

                        Target.Add(new TargetBuffer()
                        {
                            target = attackableEntities[index],
                        } );

                    }
                }

            }
        }
    }
   

}
