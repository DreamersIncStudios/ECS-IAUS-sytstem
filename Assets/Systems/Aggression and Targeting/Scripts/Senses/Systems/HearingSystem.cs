using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

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
            HearingChunk = GetArchetypeChunkComponentType<Hearing>(false),
            TransformChunk = GetArchetypeChunkComponentType<LocalToWorld>(true),
            SoundEmitters = SoundEmitters.ToComponentDataArray<SoundEmitter>(Allocator.TempJob),
            SoundPosition = SoundEmitters.ToComponentDataArray<LocalToWorld>(Allocator.TempJob)
            }.ScheduleParallel(Listeners, systemDeps);
            _entityCommandBufferSystem.AddJobHandleForProducer(systemDeps);

            Dependency = systemDeps;

        }
    }


    public struct SoundSystem : IJobChunk
    {
        public ArchetypeChunkComponentType<Hearing> HearingChunk;
        [ReadOnly] public ArchetypeChunkComponentType<LocalToWorld> TransformChunk;
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
                List<SoundData> sounds = new List<SoundData>();
                for (int j = 0; j < SoundEmitters.Length; j++)
                {
                    if (SoundEmitters[j].Sound == SoundType.Ambient)
                    {
                        float dist = Vector3.Distance(position.Position, SoundPosition[j].Position);
                        if (dist > 0)
                        {
                            sounds.Add(new SoundData()
                            {
                                soundlevel = SoundEmitters[j].SoundLevel - 20 * Mathf.Log10(dist)
                            });
                        }
                    }
                }

                float totalnoise = new float();
                foreach (SoundData sound in sounds) {
                    totalnoise += sound.SoundPressureRMS;
                
                }
                hearing.AmbientNoiseLevel = 20 * Mathf.Log10(Mathf.Pow(totalnoise,.5f) / 20);

                Hearings[i] = hearing;
            }
        }

        public struct SoundData {
            public float soundlevel;
            public float SoundPressureRMS
            {
                get {
                    float pressure = new float();
                    pressure =Mathf.Pow(Mathf.Pow(10, (soundlevel / 20)) * 20,2);
                    return pressure;
                }
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