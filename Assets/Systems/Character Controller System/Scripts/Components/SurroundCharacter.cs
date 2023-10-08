using System;
using System.Collections;
using System.Collections.Generic;
using AISenses.VisionSystems;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DreamersInc.CombatSystem
{
    public struct SurroundCharacter : IComponentData
    {
        public FixedList512Bytes<AttackPosition> PositionsSurroundingCharacter;
        public bool IgnoreCharacter {
            get
            {
                for (var i = 0; i < PositionsSurroundingCharacter.Length; i++)
                {
                    if (!PositionsSurroundingCharacter[i].Occupied)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }

    public struct createTag : IComponentData
    {
    }

    public struct AttackPosition : IEquatable<AttackPosition>
    {
        public float3 Position;
        public bool Occupied;

        public AttackPosition(float3 position, bool occupied)
        {
            Position = position;
            Occupied = occupied;
        }

        public bool Equals(AttackPosition other)
        {
            return Position.Equals(other.Position) && Occupied == other.Occupied;
        }

        public override bool Equals(object obj)
        {
            return obj is AttackPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Position, Occupied);
        }
    }

  //  [UpdateInGroup(typeof(VisionTargetingUpdateGroup))] 
    public partial struct SurroundCharacterSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }
        public void OnDestroy(ref SystemState state)
        {
        }
      [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            new CreatePositionList(){CommandBufferParallel = ecb.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter()}.ScheduleParallel();
            new UpdatePositionList().ScheduleParallel();
        }

   [BurstCompile]
        private partial struct CreatePositionList : IJobEntity
        {
            private const int NumberOfPoints = 8; // Change this to the desired number of points
            public EntityCommandBuffer.ParallelWriter CommandBufferParallel;
            
            // Calculate the angle between each point
            private const double AngleIncrement = 360.0 / NumberOfPoints;
            void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref LocalTransform transform, ref SurroundCharacter surroundCharacter, createTag tag)
            {
                surroundCharacter.PositionsSurroundingCharacter = new FixedList512Bytes<AttackPosition>();
                for (var i = 0; i < 8; i++)
                {
                    var angleInDegrees = i * AngleIncrement;
                    var angleInRadians = Mathf.PI * angleInDegrees / 180.0;
                    var pos = new float3(Mathf.Cos((float)angleInDegrees), 0, Mathf.Sin((float)angleInDegrees))*10;
                    surroundCharacter.PositionsSurroundingCharacter.Add(new AttackPosition( transform.Position+pos,false));
                }
                CommandBufferParallel.RemoveComponent<createTag>(chunkIndex, entity);
            }
        }
        [BurstCompile]
        private partial struct UpdatePositionList : IJobEntity
        {
            void Execute(ref LocalTransform transform, ref SurroundCharacter surroundCharacter)
            {
                var count = surroundCharacter.PositionsSurroundingCharacter.Length;
                var angleIncrement = 360.0 / count;
               
                for (var i = 0; i < count; i++)
                {
                    var angleInDegrees = i * angleIncrement;
                    var angleInRadians = Mathf.PI * angleInDegrees / 180.0;
                    var pos = new float3(Mathf.Cos((float)angleInDegrees), 0, Mathf.Sin((float)angleInDegrees))*10;
                    var temp = surroundCharacter.PositionsSurroundingCharacter[i];
                        temp.Position = transform.Position+pos;
                        surroundCharacter.PositionsSurroundingCharacter[i] = temp;
                }
            }
        }
    }
}