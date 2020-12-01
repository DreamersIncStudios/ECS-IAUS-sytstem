using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;
using DreamersStudio.CameraControlSystem;
using UnityStandardAssets.CrossPlatformInput;
using AISenses;

namespace DreamersStudio.TargetingSystem
{

    /// <summary>
    /// Need to Create a reactive system to operate camera  based off look at target component 
    /// </summary>
    public class TargetingSystem : SystemBase
    {
        private EntityQuery Targetters;
        private EntityQuery Targets;
    
        protected override void OnCreate()
        {
            base.OnCreate();
            Targetters = GetEntityQuery(new EntityQueryDesc() { 
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(TargetBuffer)), ComponentType.ReadOnly(typeof(LocalToWorld)),
                ComponentType.ReadOnly(typeof(Vision))}
            });
            Targets = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(Targetable)), ComponentType.ReadOnly(typeof(LocalToWorld)) }
            });
            ChangeDelay = new float();
        }

        float ChangeDelay;
        bool IsTargeting => CrossPlatformInputManager.GetAxis("Target Trigger") > .3f;
        bool PausingBetweenChange => ChangeDelay > 0.0f;
        bool ChangeTargetNeg => CrossPlatformInputManager.GetAxis("Change Target") < -.5f;
        bool ChangeTargetPos => CrossPlatformInputManager.GetAxis("Change Target") > .5f;

        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new GetTargetsList()
            {
                BufferChunk = GetArchetypeChunkBufferType<TargetBuffer>(false),
                PositionChunk = GetArchetypeChunkComponentType<LocalToWorld>(true),
                TargetablesArray = Targets.ToComponentDataArray<Targetable>(Allocator.TempJob),
                TargetPositions = Targets.ToComponentDataArray<LocalToWorld>(Allocator.TempJob)
            }.ScheduleParallel(Targetters, systemDeps);

         
            Dependency = systemDeps;
            Dependency.Complete();



            Entities.WithoutBurst().
                ForEach((ref Player_Control PC, ref DynamicBuffer<TargetBuffer> buffer, ref LookAtTarget lookAt) =>
            {

                if (buffer.Length == 0)
                    return;

                if (PausingBetweenChange) {
                    ChangeDelay -= Time.DeltaTime;
                    return;                
                }
                CameraControl.Instance.isTargeting = IsTargeting;
                if (!IsTargeting)
                {
                    lookAt.BufferIndex = 0;
                }
                else
                {
                    if (ChangeTargetNeg)
                    {
                        lookAt.BufferIndex--;
                        if (lookAt.BufferIndex < 0)
                            lookAt.BufferIndex = buffer.Length - 1;
                        ChangeDelay = .35f;
                    }

                    if (ChangeTargetPos)
                    {
                        lookAt.BufferIndex++;
                        if (lookAt.BufferIndex > buffer.Length - 1)
                            lookAt.BufferIndex = 0;
                        ChangeDelay = .75f;

                    }
                }
            }).Run();
          
          

        }





    }
    /// <summary>
    /// need to add stat based range 
    /// need to order list based on distance 
    /// </summary>
    public struct GetTargetsList : IJobChunk
    {
        public ArchetypeChunkBufferType<TargetBuffer> BufferChunk;
        [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> PositionChunk;
      //  [ReadOnly] public ArchetypeChunkComponentType<Vision> VisionChunk;

        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<Targetable> TargetablesArray;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<LocalToWorld> TargetPositions;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            BufferAccessor<TargetBuffer> Buffers = chunk.GetBufferAccessor(BufferChunk);
            NativeArray<LocalToWorld> Positions = chunk.GetNativeArray(PositionChunk);
       //     NativeArray<Vision> Visions = chunk.GetNativeArray(VisionChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                DynamicBuffer<TargetBuffer> Target = Buffers[i];
                Target.Clear();
                LocalToWorld Pos = Positions[i];
            //    Vision vision = Visions[i];
                for (int j = 0; j < TargetablesArray.Length; j++)
                {
                    float dist = Vector3.Distance(Pos.Position, TargetPositions[j].Position);
                    if(dist<100) // Create a character skill/stat for range or determine a hardcode number
                    {

                        Vector3 dir = ((Vector3)TargetPositions[j].Position - (Vector3)Pos.Position).normalized;
                        float Output = new float();
                        if (dir.x >= 0)
                        {
                            Output = Vector3.Angle(Vector3.forward, dir);
                          
                        }
                        if (dir.x < 0)
                        {
                            Output = Vector3.Angle(Vector3.forward, dir);
                         
                        }
                        Target.Add(new TargetBuffer()
                        {
                            target = new Target()
                            {
                                isFriendly = IsFriendly(TargetablesArray[j].TargetType, TargetType.Human),
                                CameraAngle = Output,
                                ID = TargetablesArray[j].ID
                            }
                        }); ; ;
                        
                    
                    }
                }


            }
        }
        public bool IsFriendly(TargetType targetType, TargetType Looker)
        {
            bool answer = false;
            switch (targetType)
            {
                case TargetType.Human:
                    switch (Looker)
                    {
                        case TargetType.Human:
                            answer = true;
                            break;
                        case TargetType.Angel:
                            answer = true;
                            break;
                        case TargetType.Daemon:
                            answer = false;
                            break;
                    }

                    break;
                case TargetType.Angel:
                    break;
                case TargetType.Daemon:
                    break;
            }
            return answer;
        }
    }

    [UpdateAfter(typeof(TargetingSystem))]
    public class CameraSync : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.WithChangeFilter<LookAtTarget>().WithoutBurst()
              .ForEach((ref Player_Control PC, ref DynamicBuffer<TargetBuffer> buffer, ref LookAtTarget lookAt) =>
              {
                  if (buffer.Length == 0)
                      return;

                  CameraControl.Instance.Target.m_XAxis.Value = buffer[lookAt.BufferIndex].target.CameraAngle;
                  GameObject temp = (GameObject)FindObjectFromInstanceID(buffer[lookAt.BufferIndex].target.ID);
                  if (temp != null)
                      CameraControl.Instance.Target.LookAt = temp.transform;
              }).Run();
        }


        public static Object FindObjectFromInstanceID(int iid)
        {
            return (Object)typeof(Object)
                    .GetMethod("FindObjectFromInstanceID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    .Invoke(null, new object[] { iid });

        }
    }

}


