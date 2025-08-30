using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using System;

namespace DreamersInc.InfluenceMapSystem
{
    public struct Perceptibility : IComponentData
    {
       [SerializeField] public float Score => (visibiltyScore + MovementScore + noiseScore) / 3.0f;
        public VisibilityStates visibilityStates;
        public MovementStates movement;
        public NoiseState noiseState;

        private float test;

        float visibiltyScore =>
            visibilityStates switch { 
                VisibilityStates.Visible => 1.0f,
                VisibilityStates.Concealed => 1.0f,
                VisibilityStates.Camo => 1.0f,
                VisibilityStates.Standing_Out => 2.0f,
                VisibilityStates.Hidden  => 1.0f,
                _ => throw new ArgumentOutOfRangeException(nameof(visibilityStates), $"Not expected Vision value: {visibilityStates}"),
            };
        float MovementScore => movement switch
        {
            MovementStates.Standing_Still => 1.0f,
            MovementStates.Walking => 1.0f,
            MovementStates.Sitting => 1.0f,
            MovementStates.Running => 1.0f,
            MovementStates.Crounched  => 1.0f,
            _ => throw new ArgumentOutOfRangeException(nameof(movement), $"Not expected Vision value: {movement}"),

        };
        float noiseScore=> noiseState switch { 
                NoiseState.Normal => 1.0f,
            NoiseState.Silent => 1.0f,
            NoiseState.Muffled => 1.0f,
            NoiseState.Sneaking => 1.0f,
            NoiseState.Yelling => 1.0f,

            _ => throw new ArgumentOutOfRangeException(nameof(noiseState), $"Not expected Vision value: {noiseState}"),

        };

    }

    public struct ChangePerceptiopnStates: IComponentData {
        public VisibilityStates visibilityStates;
        public MovementStates movement;
        public NoiseState noiseState;
    }

    public partial class ChangePerceptionSystem : SystemBase
    {
 
        protected override void OnUpdate()
        {

            var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var ECB = ecbSingleton.CreateCommandBuffer(World.Unmanaged).AsParallelWriter();


            Entities.WithBurst().ForEach((Entity entity, int entityInQueryIndex, ref Perceptibility perception, ref ChangePerceptiopnStates change) => {
                perception.visibilityStates = change.visibilityStates;
                perception.movement = change.movement;
                perception.noiseState = change.noiseState;
                ECB.RemoveComponent<ChangePerceptiopnStates>(entityInQueryIndex, entity);
            }).ScheduleParallel();
            
        }
    }


    public enum VisibilityStates { 
        Visible, Concealed, Hidden, Camo, Standing_Out
    }
    public enum MovementStates { Standing_Still, Walking, Sitting, Running, Crounched }
    public enum NoiseState {  Normal, Silent, Muffled, Sneaking, Yelling}


}