using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

namespace AISenses.HearingSystem
{
    public class HearingSystem : SystemBase
    {
        private EntityQuery SoundEmitters;
        private EntityQuery Listeners;
        EntityCommandBufferSystem _entityCommandBufferSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            Listeners = GetEntityQuery(new EntityQueryDesc() { 
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Hearing)), ComponentType.ReadOnly(typeof(LocalToWorld))}
            });
            SoundEmitters = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SoundEmitter)), ComponentType.ReadOnly(typeof(LocalToWorld)) }
            }
            );
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        }
        protected override void OnUpdate()
        {
            JobHandle systemDeps = Dependency;
            systemDeps = new SoundSystem() { 
            HearingChunk = GetComponentTypeHandle<Hearing>(false),
            TransformChunk = GetComponentTypeHandle<LocalToWorld>(true),
            SoundEmitters = SoundEmitters.ToComponentDataArray<SoundEmitter>(Allocator.TempJob),
            SoundPosition = SoundEmitters.ToComponentDataArray<LocalToWorld>(Allocator.TempJob)
            }.ScheduleParallel(Listeners, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;

        }
    }


    public struct SoundSystem : IJobChunk
    {
        public ComponentTypeHandle<Hearing> HearingChunk;
        [ReadOnly] public ComponentTypeHandle<LocalToWorld> TransformChunk;
        [ReadOnly] [DeallocateOnJobCompletion]public NativeArray<SoundEmitter> SoundEmitters;
        [ReadOnly] [DeallocateOnJobCompletion]public NativeArray<LocalToWorld> SoundPosition;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Hearing> Hearings = chunk.GetNativeArray(HearingChunk);
            NativeArray<LocalToWorld> toWorlds = chunk.GetNativeArray(TransformChunk);

            for (int i = 0; i < chunk.Count; i++)
            {
                Hearing hearing = Hearings[i];
                LocalToWorld position = toWorlds[i];
                List<AmbientSoundData> ambientSounds = new List<AmbientSoundData>();
                List<DetectedSoundData> alertSounds = new List<DetectedSoundData>();
                List<DetectedSoundData> alarmSounds = new List<DetectedSoundData>();
                for (int j = 0; j < SoundEmitters.Length; j++)
                {
                 
                        float dist = Vector3.Distance(position.Position, SoundPosition[j].Position);
                    switch (SoundEmitters[j].Sound)
                    {
                        case SoundType.Ambient:
                            if (dist >= 1)
                            {
                                ambientSounds.Add(new AmbientSoundData()
                                {
                                    soundlevel = SoundEmitters[j].SoundLevel - 20 * Mathf.Log10(dist) //>= 0? SoundEmitters[j].SoundLevel - 20 * Mathf.Log10(dist): 0
                                });
                            }
                            else {
                                ambientSounds.Add(new AmbientSoundData()
                                {
                                    soundlevel = SoundEmitters[j].SoundLevel 
                                });
                            }
                            break;
                        case SoundType.Alarm:
                            if (dist > 0)
                            {
                                alarmSounds.Add(new DetectedSoundData()
                                {
                                    SoundLocation = SoundPosition[j].Position,
                                    dist= dist,
                                });;
                            }
                            break;
                        case SoundType.Suspicious:
                            if (dist > 0)
                            {
                                alertSounds.Add(new DetectedSoundData()
                                {
                                    SoundLocation = SoundPosition[j].Position,
                                    dist=dist,
                                });
                            }

                            break;
                    }

                }

                float totalAmbientNoise = new float();
                //float totalAlertNoise = new float();
                //float totalAlarmNoise = new float();

                foreach (AmbientSoundData sound in ambientSounds) {
                    totalAmbientNoise = Mathf.Sqrt( Mathf.Pow(totalAmbientNoise,2) + Mathf.Pow(sound.SoundPressureRMS,2));
                
                }

               int ambientNoise = hearing.AmbientNoiseLevel = (int)(20 * Mathf.Log10(totalAmbientNoise / 20  ));
                foreach (DetectedSoundData sound in alertSounds)
                {
                    if (SoundAboveListenerAmbientNoise(sound.dist, ambientNoise, sound.soundlevel, out float level)) 
                    {
                        DetectedSoundData temp = sound; temp.AboveAmbientAmount = level;
                      alertSounds[  alertSounds.IndexOf(sound)] = temp;
                    }
                }
                foreach (DetectedSoundData sound in alarmSounds)
                {
                    if (SoundAboveListenerAmbientNoise(sound.dist, ambientNoise, sound.soundlevel, out float level)) {
                        DetectedSoundData temp = sound; temp.AboveAmbientAmount = level;
                       alarmSounds[alertSounds.IndexOf(sound)] = temp;
                    }

                }
                //hearing.AlertNoiseLevel = 20 * Mathf.Log10(Mathf.Pow(totalAlertNoise, .5f) / 20);
                //hearing.AlarmNoiseLevel = 20 * Mathf.Log10(Mathf.Pow(totalAlarmNoise, .5f) / 20);


                Hearings[i] = hearing;
            }
        }

        public struct AmbientSoundData {
            public float soundlevel;
            public float SoundPressureRMS
            {
                get {
                    float pressure  =Mathf.Pow(10, (soundlevel / 20)) * 20;
                    return pressure;
                }
            }
        }
        public struct DetectedSoundData
        {
            public int soundlevel;
            public float dist;
            public float3 SoundLocation;
            public float AboveAmbientAmount;

        }
        public bool SoundAboveListenerAmbientNoise( float Distance, int AmbientNoiseLevel, int NoiseDB, out float LevelAboveAmbient) {
             float NoiseAtListener = NoiseDB - 20 * Mathf.Log10(Distance);
            bool output = (5+AmbientNoiseLevel) < NoiseAtListener;
            if (output)
            {
                LevelAboveAmbient = NoiseAtListener - AmbientNoiseLevel;
            }
            else
            { LevelAboveAmbient = 0; }
            return output;
   
        }


    }
}