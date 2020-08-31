﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.AI;
using SpawnerSystem.WorldLevel;
using Unity.Collections;
using Unity.Mathematics;
using SpawnerSystem.ScriptableObjects;
using Utilities;
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

                        Entities.ForEach((Entity SPEntity, ref NPCSpawnTag Tag, ref LocalToWorld transform) =>
                    {
                        float3 displacementFromCenterOfArea = transform.Position - TempArea.CenterPostion;

                        if (Mathf.Abs(displacementFromCenterOfArea.x) < TempArea.MaxDisplacementFromCenter.x && Mathf.Abs(displacementFromCenterOfArea.y) < TempArea.MaxDisplacementFromCenter.y && Mathf.Abs(displacementFromCenterOfArea.z) < TempArea.MaxDisplacementFromCenter.z)
                        {
                            // Separate in to 3 functions
                      
                            if (GlobalFunctions.RandomPoint(transform.Position, 10.5f, out Vector3 FemalePoint))
                            {
                                // Move RandomPoint to Utility Class
                                GenericNPC.SpawnGO(Gender.Female, FemalePoint);
                            }
                                
                            if (GlobalFunctions.RandomPoint(transform.Position, 10.5f, out  Vector3 MalePoint))
                            {
                                GenericNPC.SpawnGO(Gender.Male, MalePoint);

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
                                        if (GlobalFunctions.RandomPoint(transform.Position, 10.5f, out  Vector3 SpawnPoint))
                                        {
                                            EnemyDatabase.GetEnemy(Buffer[cnt].spawnData.SpawnID).Spawn(SpawnPoint);
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
       

    }


}