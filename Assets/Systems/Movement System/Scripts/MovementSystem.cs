﻿
using UnityEngine;
using UnityEngine.AI;
using Unity.Entities;
using Components.MovementSystem;
using Unity.Transforms;
using IAUS.Core;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace IAUS.ECS.System
{
   //[UpdateAfter(typeof(IAUS_UpdateState))]
    public class MovementSystem : SystemBase
    {
        private EntityQuery Mover;


        protected override void OnCreate()
        {
            base.OnCreate();
            Mover = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Movement)), ComponentType.ReadOnly(typeof(LocalToWorld))}

            });

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;

            systemDeps = new UpdateDistanceRemaining() {
                MovementChunk = GetArchetypeChunkComponentType<Movement>(false),
                CurrentPointChunk = GetArchetypeChunkComponentType<LocalToWorld>(true)
            }.ScheduleParallel(Mover, systemDeps);

            Dependency = systemDeps;

            Entities.WithoutBurst().ForEach((NavMeshAgent Agent, ref Movement move) =>
            {
                if (move.CanMove)
                {
                    //rewrite with a set position bool;
                    if (move.SetTargetLocation)
                    {
                        Agent.SetDestination(move.TargetLocation);
                        Agent.isStopped = false;
                        move.SetTargetLocation = false;

                    }



                    if (Agent.hasPath)
                    {
                        if (move.WithinRangeOfTargetLocation)
                        {
                            // need to improve logic for picking a location to stand at 
                             //Agent.ResetPath();
                            move.CanMove = false;
                           // Agent.isStopped = true;
                        }
                    }
                }
                else
                {
                    Agent.isStopped = true;

                }


            }).Run();


        }

        public struct UpdateDistanceRemaining : IJobChunk
        {
            public ArchetypeChunkComponentType<Movement> MovementChunk;
            [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> CurrentPointChunk;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                NativeArray<Movement> Movements = chunk.GetNativeArray(MovementChunk);
                NativeArray<LocalToWorld> CurPositions = chunk.GetNativeArray(CurrentPointChunk);

                for (int i = 0; i < chunk.Count; i++)
                {
                    Movement movement = Movements[i];
                    LocalToWorld CurPos = CurPositions[i];
                    movement.DistanceRemaining = Vector3.Distance(movement.TargetLocation, CurPos.Position);

                    Movements[i] = movement;
                }

            }
        }
    }

}