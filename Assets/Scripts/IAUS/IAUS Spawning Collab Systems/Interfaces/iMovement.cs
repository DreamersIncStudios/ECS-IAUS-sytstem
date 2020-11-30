using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
namespace IAUS.SpawnerSystem.interfaces
{

    public interface iMovement
    {
        float MaxSpeed { get; set; }
        float StoppingDistance { get; set; }
        float Acceleration {get;set;}
        bool UseNavMeshAgent { get; set; }
        float Height { get; set; }
        float Radius { get; set; }
        float3 Offset { get; }
    }
}
