using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using IAUS.ECS.Component;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using DreamersInc.InflunceMapSystem;
using PixelCrushers.LoveHate;

namespace IAUS.ECS.Systems
{
    [BurstCompile]
    public struct GetDistanceToNextPoint<T> : IJobChunk
    where T : unmanaged, MovementState
    {
        public ComponentTypeHandle<T> MoveChunk;
        [ReadOnly] public ComponentTypeHandle<LocalToWorld> TransformChunk;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<T> MovesStyles = chunk.GetNativeArray(MoveChunk);
            NativeArray<LocalToWorld> toWorlds = chunk.GetNativeArray(TransformChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                T patrol = MovesStyles[i];
                patrol.distanceToPoint = Vector3.Distance(patrol.CurWaypoint.Position, toWorlds[i].Position);
                if (patrol.Complete)
                    patrol.distanceToPoint = 0.0f;
                MovesStyles[i] = patrol;
            }

        }
    }


    [BurstCompile]
    public struct CompletionChecker<T, A> : IJobChunk
           where T : unmanaged, MovementState
           where A : unmanaged, IComponentData

    {
        public ComponentTypeHandle<T> MoveChunk;
        public ComponentTypeHandle<Wait> WaitChunk;
        [ReadOnly] public BufferTypeHandle<TravelWaypointBuffer> WaypointChunk;
        [ReadOnly] public EntityTypeHandle EntityChunk;
        public EntityCommandBuffer.ParallelWriter Buffer;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<T> Moves = chunk.GetNativeArray(MoveChunk);
            NativeArray<Wait> Waits = chunk.GetNativeArray(WaitChunk);
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityChunk);
            BufferAccessor<TravelWaypointBuffer> WaypointBuffers = chunk.GetBufferAccessor(WaypointChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                T move = Moves[i];
                Wait wait = Waits[i];

                if (move.Complete)
                {
                    Buffer.RemoveComponent<A>(chunkIndex, entities[i]);
                    DynamicBuffer<TravelWaypointBuffer> waypointBuffer = WaypointBuffers[i];

                    move.Status = ActionStatus.CoolDown;
                    move.ResetTime = move.CurWaypoint.TimeToWaitatWaypoint;
                    //Todo Add info on next travel point here
                    wait.Timer = wait.StartTime = move.CurWaypoint.TimeToWaitatWaypoint;
                    if (move.TravelInOrder)
                    {
                        move.WaypointIndex++;
                        if (move.WaypointIndex >= move.NumberOfWayPoints)
                            move.WaypointIndex = 0;
                    }
                    else { 
                        move.WaypointIndex= (move.WaypointIndex+1) % move.NumberOfWayPoints;
                    }
                    move.CurWaypoint = waypointBuffer[move.WaypointIndex].WayPoint;

                    Moves[i] = move;
                    Waits[i] = wait;

                }
            }
        }
    }

    //TODO rework job
    //  [BurstCompile]
    public struct CheckThreatAtWaypoint<T> : IJobChunk
        where T : unmanaged, MovementState

    {
        public BufferTypeHandle<TravelWaypointBuffer> PatrolBuffer;
        [ReadOnly] public ComponentTypeHandle<IAUSBrain> IAUSBrainChunk;
        public ComponentTypeHandle<Patrol> PatrolChunk;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            BufferAccessor<TravelWaypointBuffer> bufferAccess = chunk.GetBufferAccessor(PatrolBuffer);
            NativeArray<IAUSBrain> Brains = chunk.GetNativeArray(IAUSBrainChunk);
            NativeArray<Patrol> patrols = chunk.GetNativeArray(PatrolChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                DynamicBuffer<TravelWaypointBuffer> buffer = bufferAccess[i];
                Patrol patrol = patrols[i];

                for (int j = 0; j < buffer.Length; j++)
                {
                    TravelWaypointBuffer point = buffer[j];
                    point.WayPoint.InfluenceAtPosition = InfluenceGridMaster.Instance.grid.GetGridObject(point.WayPoint.Position).GetValueNormalized(LoveHate.factionDatabase.GetFaction(Brains[i].factionID));
                    buffer[j] = point;
                    if (j == patrol.WaypointIndex)
                    {
                        patrol.CurWaypoint = point.WayPoint;
                    }
                }

                patrols[i] = patrol;

            }
        }
    }

}
