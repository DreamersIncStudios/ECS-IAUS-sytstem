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

namespace AISenses
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public class HearingSystem : SystemBase
    {
        private EntityQuery SoundEmitters;
        private EntityQuery Listeners;
        EntityCommandBufferSystem _entityCommandBufferSystem;
       

        protected override void OnCreate()
        {
            base.OnCreate();
            Listeners = GetEntityQuery(new EntityQueryDesc() { 
                All = new ComponentType[] { ComponentType.ReadWrite(typeof(Hearing)), ComponentType.ReadOnly(typeof(LocalToWorld)),
                ComponentType.ReadOnly(typeof(JobTempC)), ComponentType.ReadWrite(typeof(AlertLevel))
                }
            });
            SoundEmitters = GetEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] { ComponentType.ReadOnly(typeof(SoundEmitter)), ComponentType.ReadOnly(typeof(LocalToWorld)) }
            }
            );
            _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            SoundResponses.CreateSoundResponseDictionary();
            
        }
  

        float interval = 1.0f;
        bool runUpdate => interval <= 0.0f;

        protected override void OnUpdate()
        {
            //if (runUpdate)
            //{
            //    JobHandle systemDeps = Dependency;
            //    systemDeps = new SoundSystem()
            //    {
            //        HearingChunk = GetComponentTypeHandle<Hearing>(false),
            //        TransformChunk = GetComponentTypeHandle<LocalToWorld>(true),
            //        SoundEmitters = SoundEmitters.ToComponentDataArray<SoundEmitter>(Allocator.TempJob),
            //        SoundPosition = SoundEmitters.ToComponentDataArray<LocalToWorld>(Allocator.TempJob),
            //        JobChunk = GetComponentTypeHandle<JobTempC>(true),
            //        AlertChunk = GetComponentTypeHandle<AlertLevel>(false)
            //    }.ScheduleParallel(Listeners, systemDeps);
            //    _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            //    Dependency = systemDeps;
            //    interval = 1.0f;

            //}
            //else
            //{
            //    interval -= 1 / 60.0f;
            //}
        }
    }


    public struct SoundSystem : IJobChunk
    {
        public ComponentTypeHandle<Hearing> HearingChunk;
        public ComponentTypeHandle<AlertLevel> AlertChunk;

        [ReadOnly] public ComponentTypeHandle<LocalToWorld> TransformChunk;
        [ReadOnly] public ComponentTypeHandle<JobTempC> JobChunk;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<SoundEmitter> SoundEmitters;
        [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<LocalToWorld> SoundPosition;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Hearing> Hearings = chunk.GetNativeArray(HearingChunk);
            NativeArray<AlertLevel> AlertLevels = chunk.GetNativeArray(AlertChunk);

            NativeArray<LocalToWorld> toWorlds = chunk.GetNativeArray(TransformChunk);
            NativeArray<JobTempC> JobsList = chunk.GetNativeArray(JobChunk);
            Dictionary<int2, SoundResponse> SoundResponseDictionary = SoundResponses.SoundResponseDictionary;
            for (int i = 0; i < chunk.Count; i++)
            {
                Hearing hearing = Hearings[i];
                AlertLevel alertLevel = AlertLevels[i];
                LocalToWorld position = toWorlds[i];
                List<AmbientSoundData> ambientSounds = new List<AmbientSoundData>();
                List<DetectedSoundData> AllOtherSound = new List<DetectedSoundData>();
                for (int j = 0; j < SoundEmitters.Length; j++)
                {

                    float dist = Vector3.Distance(position.Position, SoundPosition[j].Position);
                    if (SoundEmitters[j].Sound == SoundTypes.Ambient)
                    {

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
                    }
                    else {

                        if (dist > 0)
                        {
                            AllOtherSound.Add(new DetectedSoundData()
                            {
                                SoundLocation = SoundPosition[j].Position,
                                dist = dist,
                                soundlevel = SoundEmitters[j].SoundLevel,
                                soundEmitter = SoundEmitters[j],
                            }); ;
                        }
                    }


                }

                float totalAmbientNoise = new float();


                foreach (AmbientSoundData sound in ambientSounds) {
                    totalAmbientNoise = Mathf.Sqrt(Mathf.Pow(totalAmbientNoise, 2) + Mathf.Pow(sound.SoundPressureRMS, 2));

                }

                int ambientNoise = hearing.AmbientNoiseLevel = (int)(20 * Mathf.Log10(totalAmbientNoise / 20));

                for (int j = 0; j < AllOtherSound.Count; j++)

                {
                    if (SoundAboveListenerAmbientNoise(AllOtherSound[j].dist, ambientNoise, AllOtherSound[j].soundlevel, out float level))
                    {
                        DetectedSoundData temp = AllOtherSound[j];
                        temp.AmountAboveAmbient = level;
                        AllOtherSound[j] = temp;
                    }
                    else
                    {
                        AllOtherSound.RemoveAt(j);
                    }

                }
                // determine what to react too?
                DetectedSoundData LoudestSound = new DetectedSoundData();
                if (AllOtherSound.Count > 0)
                {
                    foreach (var Sound in AllOtherSound)
                    {
                        if (LoudestSound.AmountAboveAmbient < Sound.AmountAboveAmbient)
                            LoudestSound = Sound;
                    }
                }
                // determine alert Levels
                if (SoundResponseDictionary.TryGetValue
                    (new int2(
                        (int)JobsList[i].job,
                        (int)LoudestSound.soundEmitter.Sound),
                    out SoundResponse response))
                {
                    // Change this to the Mod Value  once figured out
                    alertLevel.AudioAlertLevel = (int)response.ModAlertLevel(LoudestSound.SoundRatio,3);
                    alertLevel.AudioCautionLevel = (int)response.ModCautionLevel(LoudestSound.SoundRatio,3);
                }
                hearing.LocationOfSound = LoudestSound.SoundLocation;

                Hearings[i] = hearing;
                AlertLevels[i] = alertLevel;
            }
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