using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.AI;
using SpawnerSystem.WorldLevel;
using Unity.Collections;
using Unity.Mathematics;
using SpawnerSystem.ScriptableObjects;

namespace SpawnerSystem
{
    [UpdateAfter(typeof(WaveSystem.WaveMasterSystem))]
    public class SpawnSystem : ComponentSystem
    {
        public SpawnController SpawnControl;
        protected override void OnCreate()
        {
            base.OnCreate();
            SpawnControl = SpawnController.Instance;
        }
        
        protected override void OnUpdate()
        {
            if(SpawnControl==null)
            SpawnControl = SpawnController.Instance;


            if (!SpawnControl.CanSpawn)
                return;
            else
            {
                NativeArray<LocalToWorld> NPCPosition = GetEntityQuery(typeof(NpcTag), typeof(LocalToWorld)).ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
                Entities.ForEach((ref WorldSpawn Areas) =>
                {
                    //Areas.CurrentNpcCount = 0;
                    WorldSpawn TempArea = Areas;
                    TempArea.CurrentNpcCount = 0;
                    Entities.ForEach((ref NpcTag NPC, ref LocalToWorld Pos) =>
                    {
                        float3 displacementFromCenterOfArea = Pos.Position - TempArea.CenterPostion;
                        if (Mathf.Abs(displacementFromCenterOfArea.x) < TempArea.MaxDisplacementFromCenter.x && Mathf.Abs(displacementFromCenterOfArea.y) < TempArea.MaxDisplacementFromCenter.y && Mathf.Abs(displacementFromCenterOfArea.z) < TempArea.MaxDisplacementFromCenter.z)
                            TempArea.CurrentNpcCount++;

                    });
                    Entities.ForEach((ref CitizenNPCTag NPC, ref LocalToWorld Pos) =>
                    {
                        float3 displacementFromCenterOfArea = Pos.Position - TempArea.CenterPostion;
                        if (Mathf.Abs(displacementFromCenterOfArea.x) < TempArea.MaxDisplacementFromCenter.x && Mathf.Abs(displacementFromCenterOfArea.y) < TempArea.MaxDisplacementFromCenter.y && Mathf.Abs(displacementFromCenterOfArea.z) < TempArea.MaxDisplacementFromCenter.z)
                            TempArea.CurrentNpcCount++;
                        TempArea.CurrentCitizenNPCCount++;
                    });

                    Areas = TempArea;


                    if (Areas.CitizenNPCCount-Areas.CurrentCitizenNPCCount > 0)
                    {

                        Entities.ForEach((Entity SPEntity, ref EnemySpawnTag Tag, ref LocalToWorld transform) =>
                    {
                        float3 displacementFromCenterOfArea = transform.Position - TempArea.CenterPostion;

                        if (Mathf.Abs(displacementFromCenterOfArea.x) < TempArea.MaxDisplacementFromCenter.x && Mathf.Abs(displacementFromCenterOfArea.y) < TempArea.MaxDisplacementFromCenter.y && Mathf.Abs(displacementFromCenterOfArea.z) < TempArea.MaxDisplacementFromCenter.z)
                        {
                            Vector3 femalepoint;
                            if (RandomPoint(transform.Position, 10.5f, out femalepoint))
                            {
                                GenericNPC test = new GenericNPC(Gender.Female);
                                GameObject GOtest = Object.Instantiate(test.GO, femalepoint + new Vector3(0, 0, 0), transform.Rotation);
                                GOtest.GetComponent<Renderer>().material.color = test.BaseColor;
                                GOtest.GetComponent<CharacterStats>().Name = test.Name;
                            }
                                Vector3 Malepoint;
                            if (RandomPoint(transform.Position, 10.5f, out Malepoint))
                            {
                                GenericNPC test2 = new GenericNPC(Gender.Male);
                                GameObject GOtest2 = Object.Instantiate(test2.GO, Malepoint + new Vector3(0, 0, 0), transform.Rotation);
                                GOtest2.GetComponent<Renderer>().material.color = test2.BaseColor;
                                GOtest2.GetComponent<CharacterStats>().Name = test2.Name;
                            }
                        }
                    });
                    }
                    Areas = TempArea;

                    if (Areas.SpawnCount > 0)
                    {
                        Entities.ForEach((Entity SPEntity, ref EnemySpawnTag Tag, ref LocalToWorld transform) =>
                        {
                            float3 displacementFromCenterOfArea = transform.Position - TempArea.CenterPostion;

                            if (Mathf.Abs(displacementFromCenterOfArea.x) < TempArea.MaxDisplacementFromCenter.x && Mathf.Abs(displacementFromCenterOfArea.y) < TempArea.MaxDisplacementFromCenter.y && Mathf.Abs(displacementFromCenterOfArea.z) < TempArea.MaxDisplacementFromCenter.z)
                            {

                                DynamicBuffer<EnemySpawnData> Buffer = EntityManager.GetBuffer<EnemySpawnData>(SPEntity);
                                for (int cnt = 0; cnt < Buffer.Length; cnt++)
                                {
                                    if (TempArea.SpawnCount != 0)
                                    {
                                        Vector3 SpawnPoint;
                                        if (RandomPoint(transform.Position, 10.5f, out SpawnPoint))
                                        {
                                            ScriptableObjects.Enemy spawn = EnemyDatabase.GetEnemy(Buffer[cnt].spawnData.SpawnID);
                                            //add random point logic 
                                            Object.Instantiate(spawn.GO, SpawnPoint + spawn.SpawnOffset, transform.Rotation);
                                            EnemySpawnData tempData = Buffer[cnt];
                                            tempData.spawnData.SpawnCount--;
                                            SpawnControl.CountInScene++;
                                            Buffer[cnt] = tempData;
                                        }
                                    }

                                }
                            }
                        });

                    }
                });

            }
        }


        bool RandomPoint(Vector3 center, float range, out Vector3 result)
        {
            for (int i = 0; i < 30; i++)
            {
                Vector3 randomPoint = center + UnityEngine.Random.insideUnitSphere * range;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
                {
                    result = hit.position;
                    return true;
                }
            }
            result = Vector3.zero;
            return false;
        }

       

    }


}
