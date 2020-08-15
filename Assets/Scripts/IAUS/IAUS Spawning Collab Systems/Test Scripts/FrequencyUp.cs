using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace FreqTester
{
    [GenerateAuthoringComponent]
    public struct FrequencyUp : IComponentData
    {
        public int UpdateInteral;
        public int UpdatePhase;
   
    }

    //public class UpdateSystem : JobComponentSystem
    //{
    //    protected override JobHandle OnUpdate(JobHandle inputDeps)
    //    {
    //        throw new System.NotImplementedException();
    //    }
    //}
}