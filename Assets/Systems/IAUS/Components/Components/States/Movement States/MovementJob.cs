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
using Unity.Burst.Intrinsics;

namespace IAUS.ECS.Systems
{

    [BurstCompile]
    public struct CompletionChecker<T> : IJobChunk
                   where T : unmanaged, MovementState
    {
        public ComponentTypeHandle<T> MoveChunk;
        public ComponentTypeHandle<Wait> WaitChunk;
        [ReadOnly] public BufferTypeHandle<TravelWaypointBuffer> WaypointChunk;
        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            NativeArray<T> Moves = chunk.GetNativeArray(ref MoveChunk);
            NativeArray<Wait> Waits = chunk.GetNativeArray(ref WaitChunk);
            for (int i = 0; i < chunk.Count; i++)
            {
                T move = Moves[i];
                Wait wait = Waits[i];

                if (move.Complete)
                {
                    Debug.Log("finished");
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
                        move.WaypointIndex= (int)((move.WaypointIndex+1) % move.NumberOfWayPoints);
                    }

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

        public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
        {
            BufferAccessor<TravelWaypointBuffer> bufferAccess = chunk.GetBufferAccessor(ref PatrolBuffer);
            NativeArray<IAUSBrain> Brains = chunk.GetNativeArray(ref IAUSBrainChunk);
            NativeArray<Patrol> patrols = chunk.GetNativeArray(ref PatrolChunk);

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
