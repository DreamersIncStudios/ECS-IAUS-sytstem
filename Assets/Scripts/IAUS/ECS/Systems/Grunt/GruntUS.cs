using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using IAUS.Jobs;
using Unity.Jobs;


namespace IAUS.ECS.System
{
    public class GruntUS : ComponentSystem
    {
        public EntityQueryDesc Grunts = new EntityQueryDesc()
        {
            All = new ComponentType[] { typeof(MoveToComponent) , typeof(Translation)}
        };
        NativeArray<MoveToComponent> Movements;
        NativeArray<Translation> Translations;
        EntityQuery GruntsInScene;

        protected override void OnUpdate() {
            GruntsInScene = GetEntityQuery(Grunts);
            Movement();

        }
        void Movement() {
            Movements= GruntsInScene.ToComponentDataArray<MoveToComponent>(Allocator.Persistent);
            Translations = GruntsInScene.ToComponentDataArray<Translation>(Allocator.Persistent);
            NativeList<Vector3> TargetLoc = new NativeList<Vector3>(Allocator.Persistent);
            NativeArray<float> Distances = new NativeArray<float>(Translations.Length, Allocator.Persistent);
            NativeList<DistanceConsider> ConsiderationSpecs = new NativeList<DistanceConsider>(Allocator.Persistent);
            foreach (MoveToComponent movement in Movements)
            {
                TargetLoc.Add(movement.Target);
                ConsiderationSpecs.Add(movement.DS);

            }
            var job = new FindDistanceJob()
            {
                PositionsCur = Translations,
                TargetPositions = TargetLoc,
                DistanceToTarget = Distances,
                distanceConsiders = ConsiderationSpecs
                
            };
            JobHandle handle = job.Schedule(Translations.Length, 64);
            handle.Complete();
            for (int index = 0; index < ConsiderationSpecs.Length; index++)
            {
                ConsiderationSpecs[index] = job.distanceConsiders[index];
            }
            Movements.Dispose();
            Translations.Dispose();
            Distances.Dispose();
            DisposeArrays();
            ConsiderationSpecs.Dispose();
            TargetLoc.Dispose();
        }


        void DisposeArrays() {
           

        }

        

 
    }
}