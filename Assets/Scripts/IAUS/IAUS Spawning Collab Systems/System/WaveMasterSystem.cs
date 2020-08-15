using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Transforms;
using Utilities.ECS;

namespace SpawnerSystem.WaveSystem {

    public class WaveMasterSystem : ComponentSystem
    {
        /*  Rewrite with spawn tag

                     */
        public int wavecnt = 1;
        EntityManager mgr;
        protected override void OnCreate()
        {
            base.OnCreate();
            mgr = World.DefaultGameObjectInjectionWorld.EntityManager;
            m_Group = GetEntityQuery( typeof(EnemySpawnTag));

        }

        bool CurWave(WaveBuffer wave)
        {
            return wave.spawnData.Level == wavecnt;
        }


        SpawnController Control;
        EntityQuery m_Group;
        bool StartNewWave = true;
        bool spawnWave = false;
        int EnemiesInWave;
        int EnemiesSpawned;
        public int EnemiesDefeat;


        protected override void OnUpdate()
        {


            if (Input.GetKeyDown(KeyCode.Alpha5))
                spawnWave = true;
            Entities.ForEach(( DynamicBuffer<WaveBuffer> waveBuffer, ref BaseEnemySpecsForWave baseEnemy) =>
                {
                    if (StartNewWave)
                    {
                     
                        foreach (WaveBuffer wave in waveBuffer)
                        {
                            if (CurWave(wave))
                            {
                                int count = wave.spawnData.SpawnCount;
                             
                                while (count != 0)
                                {
                                    NativeArray<int> dispatched = new NativeArray<int>(1, Allocator.TempJob);
                                    var testing = new DispatchSpawnsToSpawnPointsEnemy()
                                    {
                                        SpawnCount = count,
                                        SpawnID = baseEnemy.EnemyId,
                                        count = dispatched,
                                        chunkEnemyBuffer = GetArchetypeChunkBufferType<EnemySpawnData>(),
                                        C1 = GetArchetypeChunkComponentType<EnemySpawnTag>()
                                    };

                                    JobHandle handle = testing.Schedule(m_Group);
                                    handle.Complete();

                                    count -= testing.count[0];
                                   
                                    dispatched.Dispose();
                                }
                                EnemiesInWave += wave.spawnData.SpawnCount;
                            }


                        }
                    }
            });
            StartNewWave = false;
            // write logic for spawning wave next 
            if (spawnWave) {
                Entities.ForEach((DynamicBuffer<WaveBuffer> waveBuffer, ref BaseEnemySpecsForWave baseEnemy) =>
                {
                    foreach (WaveBuffer wave in waveBuffer)
                    {
                        if (CurWave(wave))
                        {
                            int spawnnumber = wave.spawnData.MaxSpawnsPerSpawnRoutine;
                            Entities.ForEach((DynamicBuffer<EnemySpawnData> Buffer,ref EnemySpawnTag Tag, ref LocalToWorld transform) => {

                                while (spawnnumber > 0)
                                {
                                    for (int i = 0; i < Buffer.Length; i++)
                                    {
                                        if (spawnnumber == 0)
                                            goto End;

                                        EnemyDatabase.GetEnemy(Buffer[i].spawnData.SpawnID).Spawn(transform.Position);
                                        EnemySpawnData tempData = Buffer[i];
                                        tempData.spawnData.SpawnCount--;
                                        Buffer[i] = tempData;
                                        EnemiesSpawned++;
                                        spawnnumber--;

                                    }
                                }
                                End:
                                //convert to a for loop 
                                spawnnumber = 0;

                            });

                        }
                    }
                });

                    spawnWave = false;
            }
            else
            { 
                //Check how many enemy can be spawned;
            
            }

           
        }
    }


    

    [Unity.Burst.BurstCompile]
    public struct DispatchSpawnsToSpawnPointsEnemy : IJobChunk
    {
        public int SpawnID;
        public int SpawnCount;
        public NativeArray<int> count;
        public ArchetypeChunkBufferType<EnemySpawnData> chunkEnemyBuffer;
        public ArchetypeChunkComponentType<EnemySpawnTag> C1;
        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var EnemyBuffer2 = chunk.GetBufferAccessor(chunkEnemyBuffer);

            for (int cnt = 0; cnt < EnemyBuffer2.Length; cnt++)
            {
                DynamicBuffer<EnemySpawnData> EnemyBuffer = EnemyBuffer2[cnt];
                for (int i = 0; i < EnemyBuffer.Length; i++)
                {
                    if (count[0] >= SpawnCount)
                        return;

                    if (EnemyBuffer[i].spawnData.SpawnID == SpawnID)
                    {
                        EnemySpawnData temp = EnemyBuffer[i];
                        temp.spawnData.SpawnCount++;

                        EnemyBuffer[i] = temp;
                        count[0]++;

                    }
                }
            }
        }
   
    }
}